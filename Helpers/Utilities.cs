using MDS_PROJECT.Models;
using MDS_PROJECT.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;

namespace MDS_PROJECT.Helpers
{
    public class Utilities 
    {
        private readonly ApplicationDbContext db;
        private readonly IConfiguration configuration; // Configuration for accessing settings

        public Utilities(
            ApplicationDbContext _db,
            IConfiguration _configuration
        )
        {
            db = _db;
            configuration = _configuration;
        }

        public decimal StringToDecimal(string quantity)
        {
            if (string.IsNullOrEmpty(quantity))
            {
                return -1.0m;
            }
            
            if (quantity.Contains(','))
            {
                CultureInfo romanianCulture = new CultureInfo("ro-RO");
                if (decimal.TryParse(quantity, NumberStyles.Number, romanianCulture, out decimal result1))
                    return result1;
            }

            if (decimal.TryParse(quantity, out decimal result2))
                return result2;

            
            return -1.0m;
        }

        public List<Product> FilterItems(List<Product> items, decimal quantity, string measureUnit)
        {
            return items.Where(p => IsEquivalent(quantity, measureUnit, p.Quantity, p.MeasureUnit)).ToList();
        }

        public List<Product> FilterItems(List<Product> items, decimal quantity, string measureUnit, string store)
        {
            return items.Where(p => IsEquivalent(quantity, measureUnit, p.Quantity, p.MeasureUnit) && store.Equals(p.Store)).ToList();
        }


        public bool IsEquivalent(decimal quantity1, string measureUnit1, decimal quantity2, string measureUnit2)
        {
            // Convert the first quantity to a common unit (kg or L)
            if (measureUnit1.ToLower() == "kg")
            {
                quantity1 *= 1000; // Convert kg to g
                measureUnit1 = "g";
            }
            else if (measureUnit1.ToLower() == "l")
            {
                quantity1 *= 1000; // Convert L to mL
                measureUnit1 = "ml";
            }

            // Convert the second quantity to a common unit (kg or L)
            if (measureUnit2.ToLower() == "kg")
            {
                quantity2 *= 1000; // Convert kg to g
                measureUnit2 = "g";
            }
            else if (measureUnit2.ToLower() == "l")
            {
                quantity2 *= 1000; // Convert L to mL
                measureUnit2 = "ml";
            }

            // Compare the quantities
            return (Math.Abs(quantity1 - quantity2) < 0.0001m) && string.Compare(measureUnit1, measureUnit2, StringComparison.OrdinalIgnoreCase) == 0; // Small tolerance for decimal comparison
        }

        // some polimorfic functions here
        public async Task SaveToDatabase(Product product, string query)
        {
            product.Searched = query;

            if (!db.Products.Any(p => p.ItemName == product.ItemName && p.Quantity == product.Quantity && p.MeasureUnit == product.MeasureUnit && p.Store == product.Store))
            {
                db.Products.Add(product);
                await db.SaveChangesAsync();
            }
        }

        public async Task SaveToDatabase(List<Product> products, string query)
        {
            foreach (var item in products)
            {
                item.Searched = query;
                if (!db.Products.Any(p => p.ItemName == item.ItemName && p.Quantity == item.Quantity))
                {
                    db.Products.Add(item);
                }
            }

            await db.SaveChangesAsync();
        }

        public async Task<string> StartSearchScript(string scriptPath, string query, bool exactItemName)
        {
            string pythonExePath = configuration["PathVariables:PythonExePath"];
            string scriptFolderPath = configuration["PathVariables:ScriptFolderPath"] ;
            string fullScriptPath = "\"" + Path.Combine(scriptFolderPath, scriptPath) + "\"";
            string exactName = exactItemName ? "exact" : string.Empty;

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = $"{fullScriptPath} \"{query}\" {exactName}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using (Process process = Process.Start(start))
            {
                using (StreamReader outputReader = process.StandardOutput)
                using (StreamReader errorReader = process.StandardError)
                {
                    string result = await outputReader.ReadToEndAsync();
                    string error = await errorReader.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Python script error: {error}");
                    }

                    return result;
                }
            }
        } // StartSearchScript

        public List<Product> ParseResults(string results, string store)
        {
            switch (store.ToLower())
            {
                case "carrefour":
                    return ParseCarrefourResults(results);
                case "kaufland":
                    return ParseKauflandResults(results); // this is no mistake, for now the two will parse the same,
                                                          // maybe in the future will not, gonna leave it like this
                default:
                    return new List<Product>();
            }
        }

        public List<Product> ParseCarrefourResults(string results)
        {
            results = results.Replace("\r", "");
            string delimiter = new string('-', 50);
            var products = results.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            List<Product> result = new List<Product>();
            
            foreach (var product in products)
            {
                Product p = new Product();
                if (product.Trim().Length == 0)
                    continue;

                var attributes = product.Split("\n", StringSplitOptions.RemoveEmptyEntries); // [0] -> product name + others, [1] -> product price
                var processed = ProcessName(attributes[0]);
                p.Price = StringToDecimal(attributes[1]);
                p.ItemName = processed.Name;
                p.Quantity = processed.Quantity;
                p.MeasureUnit = processed.Unit;
                p.Currency = "lei";
                p.Store = "carrefour";

                result.Add(p);
            }
        
            return result;
        }

        private (string Name, decimal Quantity, string Unit) ProcessName(string str) 
        {
            var tokens = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var backIndex = 1;
            var (quantity, unit) = ProcessQuantity(tokens[tokens.Length - backIndex]);

            if (quantity < 0.0m)
            {
                backIndex = 2;
                (quantity, _) = ProcessQuantity(tokens[tokens.Length - backIndex]);
            }

            return (Name: string.Join(' ', tokens[..^backIndex]).Trim(','), Quantity: quantity, Unit: unit); 
        }

        private (decimal Quantity, string Unit) ProcessQuantity(string str)
        {
            int unitIndex;

            for (unitIndex = 0; unitIndex < str.Length; unitIndex ++)
            {
                char c = str[unitIndex];

                if (Char.IsLetter(c)) {
                    break;
                }
            }
            var quantity = StringToDecimal(str.Substring(0, unitIndex));
            var unit = str.Substring(unitIndex, str.Length - unitIndex).ToLower();
            var units = new List<string> { "l", "ml", "g", "kg", "bucata", "buc" };
            
            if (!units.Contains(unit))
                unit = "";
            
            return (Quantity: quantity, Unit: unit);
        }   

        public List<Product> ParseKauflandResults(string results)
        {
            // TODO
            return new List<Product>();
        }
    }
}