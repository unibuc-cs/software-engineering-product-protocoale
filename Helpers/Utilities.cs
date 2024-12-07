using MDS_PROJECT.Models;
using MDS_PROJECT.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
// using System.Text.RegularExpressions;
// using System.Globalization;
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
            string fullScriptPath = "\"" + Path.Combine(scriptFolderPath, exactItemName ? scriptPath.Replace(".py", "Exact.py") : scriptPath) + "\"";

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = $"{fullScriptPath} \"{query}\"",
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
            var normalizedQuantity = utils.NormalizeQuantity(quantity);
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
    }
}