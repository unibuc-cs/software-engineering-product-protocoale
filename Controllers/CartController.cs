using MDS_PROJECT.Data;
using MDS_PROJECT.Models;
using MDS_PROJECT.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;


namespace MDS_PROJECT.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly Utilities utils;

        public class CartItem
        {
            public string ItemName { get; set; }
            public string Quantity { get; set; }
            public string MeasureUnit { get; set; }
            public string CarrefourMessage { get; set; }
            public string KauflandMessage { get; set; }
            public int Multiplier { get; set; } = 1; // Default to 1 if not specified
        }

        public CartController(
            ApplicationDbContext _db, 
            Utilities _utils    
        )
        {
            db = _db;
            utils = _utils;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<CartItem> items, bool exactItemName = false)
        {
            decimal carrefourTotal = 0;
            decimal kauflandTotal = 0;

            foreach (var item in items)
            {
                var itemQuantity = utils.StringToDecimal(item.Quantity);
                item.CarrefourMessage = "Not found in Carrefour";
                item.KauflandMessage = "Not found in Kaufland";

                var existingProducts = db.Products
                                        .Where(p => p.Searched == item.ItemName)
                                        .ToList();

                List<Product> carrefourResults;
                List<Product> kauflandResults;
                bool fromDatabase = false;

                if (existingProducts.Any()) // first search in the database for products
                {
                    carrefourResults = utils.FilterItems(existingProducts, itemQuantity, item.MeasureUnit, "carrefour");
                    kauflandResults = utils.FilterItems(existingProducts, itemQuantity, item.MeasureUnit, "kaufland");
                    fromDatabase = true;
                }
                else // if there are no items in the database we start the scripts to search in sotres.
                {
                    // Defining scraper scripts and parameters
                    var carrefourTask = utils.StartSearchScript("Carrefour.py", item.ItemName, exactItemName);
                    var kauflandTask = utils.StartSearchScript("Kaufland.py", item.ItemName, exactItemName);

                    // Starting and awaiting scrapers
                    await Task.WhenAll(carrefourTask, kauflandTask);
                    
                    // Get results
                    carrefourResults = utils.ParseResults(carrefourTask.Result, "Carrefour");
                    kauflandResults = utils.ParseResults(kauflandTask.Result, "Kaufland");
                    carrefourResults = utils.FilterItems(carrefourResults, itemQuantity, item.MeasureUnit);
                    kauflandResults = utils.FilterItems(kauflandResults, itemQuantity, item.MeasureUnit);

                }


                var cheapestCarrefourItem = carrefourResults.OrderBy(p => p.Price).FirstOrDefault();
                var cheapestKauflandItem = kauflandResults.OrderBy(p => p.Price).FirstOrDefault();

                if (cheapestCarrefourItem != null)
                {
                    carrefourTotal += cheapestCarrefourItem.Price * item.Multiplier;
                    item.CarrefourMessage = $"{cheapestCarrefourItem.ItemName}: {cheapestCarrefourItem.Price} {cheapestCarrefourItem.Currency}";
                    if (!fromDatabase)
                        await utils.SaveToDatabase(cheapestCarrefourItem, item.ItemName);

                }

                if (cheapestKauflandItem != null)
                {
                    kauflandTotal += cheapestKauflandItem.Price * item.Multiplier;
                    item.KauflandMessage = $"{cheapestKauflandItem.ItemName}: {cheapestKauflandItem.Price} {cheapestKauflandItem.Currency}";
                    if (!fromDatabase)
                        await utils.SaveToDatabase(cheapestKauflandItem, item.ItemName);
                }
            } // foreach

            ViewBag.CarrefourTotal = carrefourTotal.ToString("F2", CultureInfo.InvariantCulture);
            ViewBag.KauflandTotal = kauflandTotal.ToString("F2", CultureInfo.InvariantCulture);
            ViewBag.Items = items;

            return View();
        } // Index
    }
}
