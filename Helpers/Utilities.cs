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

        public string NormalizeQuantity(string quantity)
        {
            if (string.IsNullOrEmpty(quantity))
            {
                return string.Empty;
            }
            return quantity.Replace(',', '.');
        }

        public List<Product> FilterItems(List<Product> items, string quantity)
        {
            var normalizedQuantities = GetEquivalentQuantities(quantity);
            return items.Where(p => normalizedQuantities.Contains(NormalizeQuantity(p.Quantity))).ToList();
        }

        public decimal ParsePrice(string price)
        {
            var cleanedPrice = Regex.Replace(price, @"[^\d,\.]", "");
            return decimal.Parse(cleanedPrice.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        // some polimorfic functions here
        public async Task SaveToDatabase(Product product, string query)
        {
            product.Searched = query;

            if (!db.Products.Any(p => p.ItemName == product.ItemName && p.Quantity == product.Quantity && p.MeasureQuantity == product.MeasureQuantity && p.Store == product.Store))
            {
                db.Products.Add(product);
                await db.SaveChangesAsync();
            }
        }

        public async Task SaveToDatabase(List<Product> products, string query)
        {
            foreach (var item in products)
            {
                var product = new Product
                {
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    MeasureQuantity = item.MeasureQuantity,
                    Price = item.Price,
                    Store = item.Store,
                    Searched = query
                };

                if (!db.Products.Any(p => p.ItemName == product.ItemName && p.Quantity == product.Quantity))
                {
                    db.Products.Add(product);
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


        public List<string> GetEquivalentQuantities(string quantity)
        {
            var normalizedQuantity = NormalizeQuantity(quantity);
            var equivalents = new List<string> { normalizedQuantity };

            if (decimal.TryParse(normalizedQuantity, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qty))
            {
                equivalents.Add((qty * 1000).ToString("F0", CultureInfo.InvariantCulture));
                equivalents.Add((qty / 1000).ToString("F3", CultureInfo.InvariantCulture));
                equivalents.Add(qty.ToString("F1", CultureInfo.InvariantCulture).Replace('.', ','));
                equivalents.Add((qty * 1000).ToString(CultureInfo.InvariantCulture));
                equivalents.Add((qty / 1000).ToString(CultureInfo.InvariantCulture));
            }

            return equivalents;
        }

        public List<Product> ParseResults(string results)
        {
            string pattern = @"Product: (.+?) (\d*[\.,]?\d+)\s*(\w+), Price: (\d+[\.,]?\d*) Lei";
            MatchCollection matches = Regex.Matches(results, pattern);
            return matches.Cast<Match>().Select(m => new Product
            {
                ItemName = m.Groups[1].Value.Trim(),
                Quantity = m.Groups[2].Value.Trim(),
                MeasureQuantity = m.Groups[3].Value.Trim(),
                Price = m.Groups[4].Value.Trim(),
                Store = "Carrefour"
            }).ToList();
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
                            Quantity = quantitySplit[0],
                            MeasureQuantity = quantitySplit[1],
                            Price = price + " Lei",
                            Store = "Kaufland"
                        });
                    }
                }
            }

            return kauflandResults;
        }
    }
}