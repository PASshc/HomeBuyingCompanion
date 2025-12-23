using System;
using System.Collections.Generic;
using System.Text;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;

namespace HomeBuyingApp.Infrastructure.Services
{
    public class CsvService : ICsvService
    {
        private const string Separator = ",";

        public string GenerateCsv(IEnumerable<Property> properties)
        {
            var sb = new StringBuilder();
            
            // Header
            sb.AppendLine("Address,City,State,ZipCode,ListingUrl,MlsNumber,ListPrice,EstimatedOffer,WillingToPay,Bedrooms,Bathrooms,SquareFeet,HasHoa,HoaFee,PropertyTaxRate,Comments,Notes,Rating,LookAt,Interest,IsArchived");

            foreach (var p in properties)
            {
                sb.Append(Escape(p.Address)).Append(Separator);
                sb.Append(Escape(p.City)).Append(Separator);
                sb.Append(Escape(p.State)).Append(Separator);
                sb.Append(Escape(p.ZipCode)).Append(Separator);
                sb.Append(Escape(p.ListingUrl)).Append(Separator);
                sb.Append(Escape(p.MlsNumber)).Append(Separator);
                sb.Append(p.ListPrice).Append(Separator);
                sb.Append(p.EstimatedOffer).Append(Separator);
                sb.Append(p.WillingToPay).Append(Separator);
                sb.Append(p.Bedrooms).Append(Separator);
                sb.Append(p.Bathrooms).Append(Separator);
                sb.Append(p.SquareFeet).Append(Separator);
                sb.Append(p.HasHoa).Append(Separator);
                sb.Append(p.HoaFee).Append(Separator);
                sb.Append(p.PropertyTaxRate).Append(Separator);
                sb.Append(Escape(p.Comments)).Append(Separator);
                sb.Append(Escape(p.Notes)).Append(Separator);
                sb.Append(p.Rating).Append(Separator);
                sb.Append(p.LookAt).Append(Separator);
                sb.Append(p.Interest).Append(Separator);
                sb.Append(p.IsArchived);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public IEnumerable<Property> ParseCsv(string csvContent)
        {
            var properties = new List<Property>();
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Skip header
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = ParseLine(line);
                if (values.Count < 21) continue; // Ensure enough columns

                try
                {
                    var p = new Property
                    {
                        Address = values[0],
                        City = values[1],
                        State = values[2],
                        ZipCode = values[3],
                        ListingUrl = values[4],
                        MlsNumber = values[5],
                        ListPrice = ParseDecimal(values[6]),
                        EstimatedOffer = ParseNullableDecimal(values[7]),
                        WillingToPay = ParseNullableDecimal(values[8]),
                        Bedrooms = ParseInt(values[9]),
                        Bathrooms = ParseDouble(values[10]),
                        SquareFeet = ParseInt(values[11]),
                        HasHoa = ParseBool(values[12]),
                        HoaFee = ParseDecimal(values[13]),
                        PropertyTaxRate = ParseDecimal(values[14]),
                        Comments = values[15],
                        Notes = values[16],
                        Rating = ParseDecimal(values[17]),
                        LookAt = ParseBool(values[18]),
                        Interest = ParseBool(values[19]),
                        IsArchived = ParseBool(values[20])
                    };
                    properties.Add(p);
                }
                catch
                {
                    // Skip malformed lines
                }
            }

            return properties;
        }

        private string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(Separator) || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        private List<string> ParseLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        values.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }
            values.Add(current.ToString());
            return values;
        }

        private decimal ParseDecimal(string value) => decimal.TryParse(value, out var result) ? result : 0;
        private decimal? ParseNullableDecimal(string value) => decimal.TryParse(value, out var result) ? result : null;
        private int ParseInt(string value) => int.TryParse(value, out var result) ? result : 0;
        private double ParseDouble(string value) => double.TryParse(value, out var result) ? result : 0;
        private bool ParseBool(string value) => bool.TryParse(value, out var result) ? result : false;
    }
}
