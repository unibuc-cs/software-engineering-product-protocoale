using MDS_PROJECT.Data;
using MDS_PROJECT.Models;
using MDS_PROJECT.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace MDS_PROJECT.Controllers
{
    // Controller for handling product-related actions
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db; // Database context
        private readonly UserManager<ApplicationUser> _userManager; // User manager for handling user-related operations
        private readonly RoleManager<IdentityRole> _roleManager; // Role manager for handling role-related operations
        private readonly IConfiguration _configuration; // Configuration for accessing settings
        private readonly Utilities utils;

        // Constructor to initialize the controller with the necessary dependencies
        public ProductController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            Utilities _utils)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            utils = _utils;
        }

        // Action to handle the search request for products in both stores
        [HttpPost]
        public async Task<IActionResult> SearchBoth(string query, string quantity, bool exactItemName)
        {
            // Return an empty view if the query is empty
            if (string.IsNullOrEmpty(query))
            {
                return View("Index");
            }

            // Check if the product is already in the database
            var existingProducts = db.Products.Where(p => p.Searched == query).ToList();
            if (existingProducts.Any())
            {
                ViewBag.CarrefourResults = existingProducts.Where(p => p.Store == "Carrefour" && (string.IsNullOrEmpty(quantity) || utils.GetEquivalentQuantities(quantity).Contains(p.Quantity))).ToList();
                ViewBag.KauflandResults = existingProducts.Where(p => p.Store == "Kaufland" && (string.IsNullOrEmpty(quantity) || utils.GetEquivalentQuantities(quantity).Contains(p.Quantity))).ToList();

                return View("Index");
            }

            // Execute the search scripts for Carrefour and Kaufland
            var carrefourTask = utils.StartSearchScript("Carrefour.py", query, exactItemName);
            var kauflandTask = utils.StartSearchScript("Kaufland.py", query, exactItemName);

            await Task.WhenAll(carrefourTask, kauflandTask);

            var carrefourResults = ParseResults(carrefourTask.Result);
            var kauflandResults = ParseKauflandResults(kauflandTask.Result);

            if (!string.IsNullOrEmpty(quantity))
            {
                var equivalentQuantities = utils.GetEquivalentQuantities(quantity);
                carrefourResults = carrefourResults.Where(p => equivalentQuantities.Contains(p.Quantity)).ToList();
                kauflandResults = kauflandResults.Where(p => equivalentQuantities.Contains(p.Quantity)).ToList();
            }

            await utils.SaveToDatabase(carrefourResults, query);
            await utils.SaveToDatabase(kauflandResults, query);

            ViewBag.CarrefourResults = carrefourResults;
            ViewBag.KauflandResults = kauflandResults;
            
            return View("Index");

        } // SearchBoth

        private List<Product> ParseResults(string results)
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

        private List<Product> ParseKauflandResults(string results)
        {
            // Console.WriteLine("SUNT IN KAUFLAND");
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

        // Action to display the initial search view
        public IActionResult Index()
        {
            // var viewModel = new SearchViewModel();
            return View();
        }

        // Action to handle errors
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
