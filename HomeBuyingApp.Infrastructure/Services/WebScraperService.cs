using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;
using HtmlAgilityPack;

namespace HomeBuyingApp.Infrastructure.Services
{
    public class WebScraperService : IWebScraperService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ZenRowsApiKey = "b7a3deaf9c40fe61d5864df460a189c9afb0b306";

        static WebScraperService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.google.com/");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
        }

        public async Task<Property> ScrapePropertyDetailsAsync(string url)
        {
            var property = new Property { ListingUrl = url };
            string html = null;
            
            try
            {
                // 0. Try ZenRows API if configured (Bypasses blocking)
                if (!string.IsNullOrEmpty(ZenRowsApiKey) && url.Contains("zillow.com"))
                {
                    var zenRowsProperty = await TryGetPropertyViaZenRows(url);
                    if (zenRowsProperty != null && !string.IsNullOrEmpty(zenRowsProperty.Address))
                    {
                        return zenRowsProperty;
                    }
                }

                // Use HttpClient to get the HTML content with proper headers if ZenRows didn't return anything
                if (string.IsNullOrEmpty(html))
                {
                    html = await _httpClient.GetStringAsync(url);
                }
                
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                
                // 1. Try to extract from JSON-LD (Schema.org)
                bool success = TryExtractJsonLd(doc, property);

                // 2. Try to extract from Next.js data (Zillow often uses this)
                if (!success || string.IsNullOrEmpty(property.Address))
                {
                    success = TryExtractNextJsData(doc, property);
                }

                // 3. Fallback or Supplement with Open Graph tags
                if (!success || string.IsNullOrEmpty(property.Address))
                {
                    ExtractOpenGraphData(doc, property);
                }
            }
            catch (Exception ex)
            {
                // Log error or just return what we have
                Console.WriteLine($"Error scraping URL: {ex.Message}");
            }

            return property;
        }

        private bool TryExtractNextJsData(HtmlDocument doc, Property property)
        {
            try
            {
                var script = doc.DocumentNode.SelectSingleNode("//script[@id='__NEXT_DATA__']");
                if (script == null) return false;

                var json = script.InnerText;
                if (string.IsNullOrWhiteSpace(json)) return false;

                var node = JsonNode.Parse(json);
                if (node == null) return false;

                // Navigate to the property details in the Next.js state
                // Structure varies, but often: props -> pageProps -> componentProps -> gdpClientCache -> ...
                // Or: props -> pageProps -> property -> ...
                
                var props = node["props"]?["pageProps"];
                if (props == null) return false;

                // Try to find property data in various locations
                var propertyData = props["componentProps"]?["gdpClientCache"]?.AsObject().FirstOrDefault().Value?["property"];
                
                if (propertyData == null)
                {
                    // Try another path
                    propertyData = props["property"];
                }

                if (propertyData != null)
                {
                    // Extract Address
                    var address = propertyData["address"];
                    if (address != null)
                    {
                        property.Address = address["streetAddress"]?.ToString() ?? property.Address;
                        property.City = address["city"]?.ToString() ?? property.City;
                        property.State = address["state"]?.ToString() ?? property.State;
                        property.ZipCode = address["zipcode"]?.ToString() ?? property.ZipCode;
                    }

                    // Extract Price
                    if (decimal.TryParse(propertyData["price"]?.ToString(), out decimal price))
                    {
                        property.ListPrice = price;
                    }

                    // Extract Beds/Baths/SqFt
                    if (double.TryParse(propertyData["bedrooms"]?.ToString(), out double beds))
                        property.Bedrooms = (int)beds;

                    if (double.TryParse(propertyData["bathrooms"]?.ToString(), out double baths))
                        property.Bathrooms = baths;

                    if (int.TryParse(propertyData["livingArea"]?.ToString(), out int sqft))
                        property.SquareFeet = sqft;
                    
                    // Extract Description
                    property.Comments = propertyData["description"]?.ToString() ?? property.Comments;

                    // Extract Images (maybe later)
                    
                    return !string.IsNullOrEmpty(property.Address);
                }
            }
            catch { /* Ignore errors */ }

            return false;
        }

        private bool TryExtractJsonLd(HtmlDocument doc, Property property)
        {
            try
            {
                var scripts = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
                if (scripts == null) return false;

                foreach (var script in scripts)
                {
                    var json = script.InnerText;
                    if (string.IsNullOrWhiteSpace(json)) continue;

                    try
                    {
                        var node = JsonNode.Parse(json);
                        if (node == null) continue;

                        // Handle array of objects (graph)
                        if (node is JsonArray array)
                        {
                            foreach (var item in array)
                            {
                                if (ProcessJsonLdNode(item, property)) return true;
                            }
                        }
                        else
                        {
                            // Handle single object or @graph
                            var graph = node["@graph"];
                            if (graph is JsonArray graphArray)
                            {
                                foreach (var item in graphArray)
                                {
                                    if (ProcessJsonLdNode(item, property)) return true;
                                }
                            }
                            else
                            {
                                if (ProcessJsonLdNode(node, property)) return true;
                            }
                        }
                    }
                    catch { /* Ignore JSON parse errors */ }
                }
            }
            catch { /* Ignore selection errors */ }

            return false;
        }

        private bool ProcessJsonLdNode(JsonNode? node, Property property)
        {
            if (node == null) return false;

            var type = node["@type"]?.ToString();
            if (string.IsNullOrEmpty(type)) return false;

            // Check for relevant types
            var relevantTypes = new[] { "SingleFamilyResidence", "RealEstateListing", "Product", "Place", "House", "Apartment", "Residence" };
            if (!relevantTypes.Any(t => type.Contains(t, StringComparison.OrdinalIgnoreCase))) return false;

            // Extract Address
            var addressNode = node["address"];
            if (addressNode != null)
            {
                property.Address = addressNode["streetAddress"]?.ToString() ?? property.Address;
                property.City = addressNode["addressLocality"]?.ToString() ?? property.City;
                property.State = addressNode["addressRegion"]?.ToString() ?? property.State;
                property.ZipCode = addressNode["postalCode"]?.ToString() ?? property.ZipCode;
            }

            // Extract Price (often in 'offers')
            var offers = node["offers"];
            if (offers != null)
            {
                // Offers can be an array or object
                if (offers is JsonArray offersArray && offersArray.Count > 0)
                {
                    var firstOffer = offersArray[0];
                    ParsePriceFromOffer(firstOffer, property);
                }
                else
                {
                    ParsePriceFromOffer(offers, property);
                }
            }

            // Extract Beds/Baths/SqFt
            // Schema.org properties: numberOfBedrooms, numberOfBathroomsTotal, floorSize
            if (node["numberOfBedrooms"] != null)
            {
                if (double.TryParse(node["numberOfBedrooms"]?.ToString(), out double beds))
                    property.Bedrooms = (int)beds;
            }

            if (node["numberOfBathroomsTotal"] != null)
            {
                if (double.TryParse(node["numberOfBathroomsTotal"]?.ToString(), out double baths))
                    property.Bathrooms = baths;
            }
            else if (node["numberOfBathrooms"] != null)
            {
                 if (double.TryParse(node["numberOfBathrooms"]?.ToString(), out double baths))
                    property.Bathrooms = baths;
            }

            var floorSize = node["floorSize"];
            if (floorSize != null)
            {
                // floorSize can be an object with 'value' or just a value
                var value = floorSize["value"]?.ToString() ?? floorSize.ToString();
                if (int.TryParse(value, out int sqft))
                    property.SquareFeet = sqft;
            }

            // Extract Description/Url
            if (string.IsNullOrEmpty(property.ListingUrl))
                property.ListingUrl = node["url"]?.ToString() ?? property.ListingUrl;
            
            // If we got at least an address, consider it a success
            return !string.IsNullOrEmpty(property.Address);
        }

        private void ParsePriceFromOffer(JsonNode? offer, Property property)
        {
            if (offer == null) return;
            var priceStr = offer["price"]?.ToString();
            if (decimal.TryParse(priceStr, out decimal price))
            {
                property.ListPrice = price;
            }
        }

        private void ExtractOpenGraphData(HtmlDocument doc, Property property)
        {
            // Extract Open Graph tags
            var ogTitle = GetMetaContent(doc, "og:title");
            var ogDescription = GetMetaContent(doc, "og:description");
            
            // Combine title and description for searching
            var combinedText = $"{ogTitle} {ogDescription}";

            // Parse Price if not already found
            if (property.ListPrice == 0) property.ListPrice = ParsePrice(combinedText);

            // Parse Beds if not already found
            if (property.Bedrooms == 0) property.Bedrooms = ParseBeds(combinedText);

            // Parse Baths if not already found
            if (property.Bathrooms == 0) property.Bathrooms = ParseBaths(combinedText);

            // Parse SqFt if not already found
            if (property.SquareFeet == 0) property.SquareFeet = ParseSqFt(combinedText);

            // Parse Address if not already found
            if (string.IsNullOrEmpty(property.Address))
            {
                ParseAddress(ogTitle, property);
            }
        }

        private string GetMetaContent(HtmlDocument doc, string propertyName)
        {
            var node = doc.DocumentNode.SelectSingleNode($"//meta[@property='{propertyName}']");
            return node?.GetAttributeValue("content", "") ?? "";
        }

        private decimal ParsePrice(string text)
        {
            // Look for $ followed by numbers and commas
            var match = Regex.Match(text, @"\$([\d,]+)");
            if (match.Success)
            {
                var cleanPrice = match.Groups[1].Value.Replace(",", "");
                if (decimal.TryParse(cleanPrice, out var price))
                {
                    return price;
                }
            }
            return 0;
        }

        private int ParseBeds(string text)
        {
            // Look for "3 bds", "3 beds", "3 bedrooms"
            var match = Regex.Match(text, @"(\d+)\s*(?:bds?|beds?|bedrooms?)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var beds))
            {
                return beds;
            }
            return 0;
        }

        private double ParseBaths(string text)
        {
            // Look for "2 ba", "2 baths", "2 bathrooms", "2.5 baths"
            var match = Regex.Match(text, @"(\d+(?:\.\d+)?)\s*(?:ba|baths?|bathrooms?)", RegexOptions.IgnoreCase);
            if (match.Success && double.TryParse(match.Groups[1].Value, out var baths))
            {
                return baths;
            }
            return 0;
        }


        private int ParseSqFt(string text)
        {
            // Look for "2,000 sqft", "2000 sq ft"
            var match = Regex.Match(text, @"([\d,]+)\s*(?:sq\s*ft|sqft|square\s*feet)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var cleanSqFt = match.Groups[1].Value.Replace(",", "");
                if (int.TryParse(cleanSqFt, out var sqft))
                {
                    return sqft;
                }
            }
            return 0;
        }

        private void ParseAddress(string title, Property property)
        {
            if (string.IsNullOrWhiteSpace(title)) return;

            // Remove site name suffix if present (e.g., " | Zillow", " - Redfin")
            var cleanTitle = Regex.Replace(title, @"\s*[|\-].*$", "");

            // Heuristic: Assume format "Address, City, State Zip"
            // Split by commas
            var parts = cleanTitle.Split(',').Select(p => p.Trim()).ToList();

            if (parts.Count >= 3)
            {
                // Last part usually contains State and Zip
                var stateZip = parts.Last();
                parts.RemoveAt(parts.Count - 1);

                // Second to last is City
                property.City = parts.Last();
                parts.RemoveAt(parts.Count - 1);

                // Remainder is Address
                property.Address = string.Join(", ", parts);

                // Parse State and Zip from the last part
                var stateZipMatch = Regex.Match(stateZip, @"([A-Z]{2})\s+(\d{5})");
                if (stateZipMatch.Success)
                {
                    property.State = stateZipMatch.Groups[1].Value;
                    property.ZipCode = stateZipMatch.Groups[2].Value;
                }
                else
                {
                    // Fallback: just take the first word as state
                    var splitStateZip = stateZip.Split(' ');
                    if (splitStateZip.Length > 0) property.State = splitStateZip[0];
                    if (splitStateZip.Length > 1) property.ZipCode = splitStateZip[1];
                }
            }
            else
            {
                // Fallback: put everything in Address
                property.Address = cleanTitle;
            }
        }

        private async Task<Property> TryGetPropertyViaZenRows(string url)
        {
            try
            {
                // Use ZenRows with autoparse=true to get JSON data directly
                var encodedUrl = System.Net.WebUtility.UrlEncode(url);
                var apiUrl = $"https://api.zenrows.com/v1/?apikey={ZenRowsApiKey}&url={encodedUrl}&js_render=true&premium_proxy=true&autoparse=true";

                var json = await _httpClient.GetStringAsync(apiUrl);
                var node = JsonNode.Parse(json);
                if (node == null) return null;

                var property = new Property { ListingUrl = url };

                // Map ZenRows Autoparse JSON to Property object
                // Note: Keys are case-sensitive and depend on ZenRows' parsing logic for Zillow
                
                // Address
                property.Address = node["address"]?.ToString() ?? node["streetAddress"]?.ToString();
                property.City = node["city"]?.ToString() ?? node["addressLocality"]?.ToString();
                property.State = node["state"]?.ToString() ?? node["addressRegion"]?.ToString();
                property.ZipCode = node["zipcode"]?.ToString() ?? node["postalCode"]?.ToString();

                // Price
                var priceStr = node["price"]?.ToString() ?? node["listPrice"]?.ToString();
                if (!string.IsNullOrEmpty(priceStr))
                {
                    priceStr = Regex.Replace(priceStr, @"[^\d.]", "");
                    if (decimal.TryParse(priceStr, out decimal price)) property.ListPrice = price;
                }

                // Details
                if (double.TryParse(node["bedrooms"]?.ToString(), out double beds)) property.Bedrooms = (int)beds;
                if (double.TryParse(node["bathrooms"]?.ToString(), out double baths)) property.Bathrooms = baths;
                
                var sqftStr = node["livingArea"]?.ToString() ?? node["floorSize"]?.ToString();
                if (!string.IsNullOrEmpty(sqftStr))
                {
                    sqftStr = Regex.Replace(sqftStr, @"[^\d]", "");
                    if (int.TryParse(sqftStr, out int sqft)) property.SquareFeet = sqft;
                }

                property.Comments = node["description"]?.ToString();

                return property;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ZenRows API Error: {ex.Message}");
                return null;
            }
        }
    }
}
