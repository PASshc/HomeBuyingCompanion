using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HomeBuyingApp.Core.Services
{
    public static class ListingTextParser
    {
        public sealed class ParsedListing
        {
            public decimal? ListPrice { get; init; }
            public decimal? Bedrooms { get; init; }
            public decimal? Bathrooms { get; init; }
            public int? SquareFeet { get; init; }
            public string? AddressLine { get; init; }
            public string? City { get; init; }
            public string? State { get; init; }
            public string? ZipCode { get; init; }
        }

        public static ParsedListing Parse(string? rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return new ParsedListing();
            }

            rawText = NormalizeText(rawText);

            // Normalize whitespace while preserving line boundaries for heuristics.
            var lines = rawText
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            var text = string.Join("\n", lines);

            var (city, state, zip) = ParseCityStateZip(text);
            var addressLine = ParseAddressLine(lines, city, state, zip);

            return new ParsedListing
            {
                ListPrice = ParsePrice(text),
                Bedrooms = ParseDecimalToken(text, @"(?<v>\d+(?:\.\d+)?)\s*(?:bd|bds|bed|beds|bedroom|bedrooms)\b") 
                          ?? ParseDecimalToken(text, @"(?:^|\n)\s*Bed(?:room)?s?\s*[:\-]?\s*(?<v>\d+(?:\.\d+)?)\b"),
                Bathrooms = ParseDecimalToken(text, @"(?<v>\d+(?:\.\d+)?)\s*(?:ba|bas|bath|baths|bathroom|bathrooms)\b")
                           ?? ParseDecimalToken(text, @"(?:^|\n)\s*Bath(?:room)?s?\s*[:\-]?\s*(?<v>\d+(?:\.\d+)?)\b"),
                SquareFeet = ParseIntToken(text, @"(?<v>[\d,]+)\s*(?:sq\s*\.?\s*ft|sqft|square\s+feet|sq\.\s*ft\.)\b")
                            ?? ParseIntToken(text, @"(?:^|\n)\s*(?:Square\s+Feet|Living\s+Area|Size)\s*[:\-]?\s*(?<v>[\d,]+)\b"),
                AddressLine = addressLine,
                City = city,
                State = state,
                ZipCode = zip
            };
        }

        private static string NormalizeText(string text)
        {
            // Common copy/paste artifacts from listing sites.
            // - non-breaking spaces
            // - bullet separators ("•")
            // - interpunct ("·")
            // - pipes ("|")
            // We normalize them to spaces so token regexes work reliably.
            var normalized = text
                .Replace('\u00A0', ' ')
                .Replace('•', ' ')
                .Replace('·', ' ')
                .Replace('|', ' ');

            // Collapse excessive whitespace but keep line breaks.
            normalized = Regex.Replace(normalized, "[\\t\\f\\v ]+", " ");
            return normalized;
        }

        private static (string? City, string? State, string? Zip) ParseCityStateZip(string text)
        {
            // e.g. "Orlando, FL 32801" or "Orlando FL 32801"
            var match = Regex.Match(
                text,
                @"(?<city>[A-Za-z][A-Za-z .'-]+?)\s*,?\s+(?<state>[A-Z]{2})\b(?:\s+(?<zip>\d{5}(?:-\d{4})?))?\b",
                RegexOptions.Multiline);

            if (!match.Success)
            {
                return (null, null, null);
            }

            return (
                match.Groups["city"].Value.Trim(),
                match.Groups["state"].Value.Trim(),
                match.Groups["zip"].Success ? match.Groups["zip"].Value.Trim() : null);
        }

        private static string? ParseAddressLine(string[] lines, string? city, string? state, string? zip)
        {
            string? rawAddress = null;

            // Prefer a line that looks like a street address.
            foreach (var line in lines.Take(20))
            {
                // Heuristic: starts with a street number and includes a common street suffix.
                if (Regex.IsMatch(
                    line,
                    @"^\s*\d+\s+.+\b(?:st|street|ave|avenue|rd|road|dr|drive|ln|lane|blvd|boulevard|ct|court|cir|circle|way|pkwy|parkway|pl|place|ter|terrace)\b",
                    RegexOptions.IgnoreCase))
                {
                    rawAddress = line;
                    break;
                }
            }

            // Fallback: any early line that starts with a number + space.
            if (rawAddress == null)
            {
                foreach (var line in lines.Take(10))
                {
                    if (Regex.IsMatch(line, @"^\s*\d+\s+\S+", RegexOptions.None))
                    {
                        rawAddress = line;
                        break;
                    }
                }
            }

            if (rawAddress == null)
            {
                return null;
            }

            // Strip city/state/zip suffix if we parsed those separately.
            // e.g. "10325 Fairway Dr, Kelseyville, CA 95451" → "10325 Fairway Dr"
            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(state))
            {
                // Build a pattern to match ", City, ST ZIP" or ", City ST ZIP" or "City, ST ZIP" etc.
                var cityEscaped = Regex.Escape(city);
                var stateEscaped = Regex.Escape(state);
                var zipPattern = !string.IsNullOrWhiteSpace(zip) ? @"\s+" + Regex.Escape(zip) : @"(?:\s+\d{5}(?:-\d{4})?)?";

                var suffixPattern = $@",?\s*{cityEscaped}\s*,?\s+{stateEscaped}{zipPattern}\s*$";
                var stripped = Regex.Replace(rawAddress, suffixPattern, string.Empty, RegexOptions.IgnoreCase).Trim();

                // Only use stripped version if it still looks like a street address.
                if (!string.IsNullOrWhiteSpace(stripped) && Regex.IsMatch(stripped, @"^\d+\s+\S+"))
                {
                    return stripped;
                }
            }

            return rawAddress;
        }

        private static decimal? ParsePrice(string text)
        {
            // Handle common formats: "$425,000" or "$1.2M" or "$950K"
            var match = Regex.Match(text, @"\$(?<num>\d{1,3}(?:,\d{3})+|\d+(?:\.\d+)?)(?<suffix>[KkMm])?\b");
            if (!match.Success)
            {
                // Fallback for sites that omit the '$' but label the price (Zillow, Realtor.com, OneHome, etc.)
                match = Regex.Match(
                    text,
                    @"\b(?:price|listed\s+for|list\s+price|asking)\s*[:\-]?\s*\$?(?<num>\d{1,3}(?:,\d{3})+|\d+(?:\.\d+)?)(?<suffix>[KkMm])?\b",
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    // OneHome format: "Price: 425000" or just a plain large number in early lines
                    match = Regex.Match(text, @"(?:^|\n)\s*(?:Price|Listing Price)[:\s]+(?<num>\d{1,3}(?:,\d{3})+|\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    
                    if (!match.Success)
                    {
                        return null;
                    }
                }
            }

            var numRaw = match.Groups["num"].Value.Replace(",", "");
            if (!decimal.TryParse(numRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return null;
            }

            var suffix = match.Groups["suffix"].Value;
            if (suffix.Equals("K", StringComparison.OrdinalIgnoreCase))
            {
                value *= 1_000m;
            }
            else if (suffix.Equals("M", StringComparison.OrdinalIgnoreCase))
            {
                value *= 1_000_000m;
            }

            return value;
        }

        private static decimal? ParseDecimalToken(string text, string pattern)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            var raw = match.Groups["v"].Value;
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return null;
        }

        private static int? ParseIntToken(string text, string pattern)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            var raw = match.Groups["v"].Value.Replace(",", "");
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return null;
        }
    }
}
