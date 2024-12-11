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
            
            CultureInfo romanianCulture = new CultureInfo("ro-RO");

            if (decimal.TryParse(quantity, NumberStyles.Number, romanianCulture, out decimal result1))
                return result1;
            
            if (decimal.TryParse(quantity, out decimal result2))
                return result2;
            
            return -1.0m;
        }

        public List<Product> FilterItems(List<Product> items, decimal quantity, string measureUnit)
        {
            return items.Where(p => IsEquivalent(quantity, measureUnit, p.Quantity, p.MeasureUnit)).ToList();
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
                measureUnit1 = "mL";
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
                measureUnit2 = "mL";
            }

            // Compare the quantities
            return (Math.Abs(quantity1 - quantity2) < 0.0001m) && string.Compare(measureUnit1, measureUnit2) == 0; // Small tolerance for decimal comparison
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
                // var product = new Product(item);
                // // {
                // //     ItemName = item.ItemName,
                // //     Quantity = item.Quantity,
                // //     MeasureUnit = item.MeasureUnit,
                // //     Price = item.Price,
                // //     Store = item.Store,
                // //     Searched = query
                // // };
                // product.Searched = query;
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
            System.Console.WriteLine($"{fullScriptPath} \"{query}\" {exactName}");

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
                        System.Console.WriteLine($"Python script error output: {error}");
                        throw new Exception($"Python script error: {error}");
                    }

                    return result;
                }
            }
        } // StartSearchScript


        public List<Product> ParseResults(string results)
        {
            string pattern = @"Product: (.+?) (\d*[\.,]?\d+)\s*(\w+), Price: (\d+[\.,]?\d*) Lei";
            MatchCollection matches = Regex.Matches(results, pattern);
            return matches.Cast<Match>().Select(m => new Product
                {
                    ItemName = m.Groups[1].Value.Trim(),
                    Quantity = StringToDecimal(m.Groups[2].Value.Trim()),
                    MeasureUnit = m.Groups[3].Value.Trim(),
                    Price = StringToDecimal(m.Groups[4].Value.Trim()),
                    Currency = "lei",
                    Store = "Carrefour"
                }).ToList();
        }

        public List<Product> ParseResults(string results, string store)
        {
            switch (store)
            {
                case string.Equals("carrefour", store, StringComparison.OrdinalIgnoreCase):
                    return ParseCarrefourResults(results);
                case string.Equals("kaufland", store, StringComparison.OrdinalIgnoreCase):
                    return ParseKauflandResults(results);
                default:
                    return new List<Product>();
            }
        }

        public List<Product> ParseCarrefourResults(string results)
        {
            string delimiter = new string("-", 50);
            var products = results.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            List<Product> result = new List<Product>();

            foreach (var product in products)
            {
                var attributes = product.Split("\n"); // [0] -> product name + others, [1] -> product price
                

            }
        
            return result;
        }

        public List<Product> ParseKauflandResults(string results)
        {
            List<Product> kauflandResults = new List<Product>();
            var lines = results.Split(new string[] { "--------------------------------" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string pattern = @"Product Name: (.+?)\r\nProduct Subtitle: (.+?)\r\nProduct Price: (\d+[\.,]?\d*)\r\nProduct Quantity: (.+)";
                Match match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    string itemName = match.Groups[1].Value.Trim() + " " + match.Groups[2].Value.Trim();
                    string price = match.Groups[3].Value.Trim();
                    string quantity = match.Groups[4].Value.Trim();

                    var quantitySplit = quantity.Split(new char[] { ' ' }, 2);
                    if (quantitySplit.Length == 2)
                    {
                        kauflandResults.Add(new Product
                        {
                            ItemName = itemName,
                            Quantity = StringToDecimal(quantitySplit[0]),
                            MeasureUnit = quantitySplit[1],
                            Price = StringToDecimal(price),
                            Currency = "lei",
                            Store = "Kaufland"
                        });
                    }
                }
            }

            return kauflandResults;
        }
    }
}