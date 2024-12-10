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
        // private readonly IConfiguration configuration;
        private readonly Utilities utils;

        /*///////////////////////////////*/
        /*-------[ Public Actions]-------*/
        /*///////////////////////////////*/

        public CartController(
            ApplicationDbContext _db, 
            // IConfiguration _configuration,
            Utilities _utils    
        )
        {
            db = _db;
            // configuration = _configuration;
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
                var existingProducts = db.Products
                                          .Where(p => p.Searched == item.ItemName)
                                          .ToList();

                var carrefourMessage = "Not found in Carrefour";
                var kauflandMessage = "Not found in Kaufland";
                var carrefourStoreItemName = string.Empty;
                var kauflandStoreItemName = string.Empty;

                if (existingProducts.Any()) // first search in the database for products
                {
                    var carrefourItems = existingProducts.Where(p => p.Store == "Carrefour").ToList();
                    var kauflandItems = existingProducts.Where(p => p.Store == "Kaufland").ToList();

                    carrefourItems = utils.FilterItems(carrefourItems, item.Quantity);
                    kauflandItems = utils.FilterItems(kauflandItems, item.Quantity);

                    var cheapestCarrefourItem = carrefourItems.OrderBy(p => utils.ParsePrice(p.Price)).FirstOrDefault();
                    var cheapestKauflandItem = kauflandItems.OrderBy(p => utils.ParsePrice(p.Price)).FirstOrDefault();

                    if (cheapestCarrefourItem != null)
                    {
                        carrefourTotal += utils.ParsePrice(cheapestCarrefourItem.Price) * item.Multiplier;
                        carrefourMessage = $"{cheapestCarrefourItem.ItemName}: {cheapestCarrefourItem.Price} Lei";
                        carrefourStoreItemName = cheapestCarrefourItem.ItemName;
                    }

                    if (cheapestKauflandItem != null)
                    {
                        kauflandTotal += utils.ParsePrice(cheapestKauflandItem.Price) * item.Multiplier;
                        kauflandMessage = $"{cheapestKauflandItem.ItemName}: {cheapestKauflandItem.Price} Lei";
                        kauflandStoreItemName = cheapestKauflandItem.ItemName;
                    }
                }
                else // if there are no items in the database we start the scripts to search in sotres.
                {
                    // Defining scraper scripts and parameters
                    var carrefourTask = utils.StartSearchScript("Carrefour.py", item.ItemName, exactItemName);
                    var kauflandTask = utils.StartSearchScript("Kaufland.py", item.ItemName, exactItemName);

                    // Starting and awaiting scrapers
                    await Task.WhenAll(carrefourTask, kauflandTask);
                    
                    // Get results
                    var carrefourResults = ParseResults(carrefourTask.Result, "Carrefour");
                    var kauflandResults = ParseResults(kauflandTask.Result, "Kaufland");

                    carrefourResults = utils.FilterItems(carrefourResults, item.Quantity);
                    kauflandResults = utils.FilterItems(kauflandResults, item.Quantity);

                    var cheapestCarrefourItem = carrefourResults.OrderBy(p => utils.ParsePrice(p.Price)).FirstOrDefault();
                    var cheapestKauflandItem = kauflandResults.OrderBy(p => utils.ParsePrice(p.Price)).FirstOrDefault();

                    if (cheapestCarrefourItem != null)
                    {
                        carrefourTotal += utils.ParsePrice(cheapestCarrefourItem.Price) * item.Multiplier;
                        carrefourMessage = $"{cheapestCarrefourItem.ItemName}: {cheapestCarrefourItem.Price} Lei";
                        carrefourStoreItemName = cheapestCarrefourItem.ItemName;
                        await utils.SaveToDatabase(cheapestCarrefourItem, item.ItemName);
                    }

                    if (cheapestKauflandItem != null)
                    {
                        kauflandTotal += utils.ParsePrice(cheapestKauflandItem.Price) * item.Multiplier;
                        kauflandMessage = $"{cheapestKauflandItem.ItemName}: {cheapestKauflandItem.Price} Lei";
                        kauflandStoreItemName = cheapestKauflandItem.ItemName;
                        await utils.SaveToDatabase(cheapestKauflandItem, item.ItemName);
                    }
                } // else

                item.CarrefourMessage = carrefourMessage;
                item.KauflandMessage = kauflandMessage;
                item.CarrefourStoreItemName = carrefourStoreItemName;
                item.KauflandStoreItemName = kauflandStoreItemName;
            } // foreach

            ViewBag.CarrefourTotal = carrefourTotal.ToString("F2", CultureInfo.InvariantCulture);
            ViewBag.KauflandTotal = kauflandTotal.ToString("F2", CultureInfo.InvariantCulture);
            ViewBag.Items = items;

            return View();
        } // Index

        /*///////////////////////////////////*/
        /*-------[ Private functions ]-------*/
        /*///////////////////////////////////*/

        [NonAction]
        private List<Product> ParseResults(string results, string store)
        {
            string pattern = store == "Carrefour"
                ? @"Product: (.+?) (\d*[\.,]?\d+)\s*(\w+), Price: (\d+[\.,]?\d*) Lei"
                : @"Product Name: (.+?)\r\nProduct Subtitle: (.+?)\r\nProduct Price: (\d+[\.,]?\d*)\r\nProduct Quantity: (.+)";

            MatchCollection matches = Regex.Matches(results, pattern);

            return matches.Cast<Match>().Select(m =>
            {
                if (store == "Carrefour")
                {
                    if (m.Groups.Count != 5)
                    {
                        Debug.WriteLine($"Unexpected match format for Carrefour: {m.Value}");
                        return null;
                    }
                    return new Product
                    {
                        ItemName = m.Groups[1].Value.Trim(),
                        Quantity = m.Groups[2].Value.Trim(),
                        MeasureQuantity = m.Groups[3].Value.Trim(),
                        Price = m.Groups[4].Value.Trim(),
                        Store = store
                    };
                }
                else
                {
                    if (m.Groups.Count != 5)
                    {
                        Debug.WriteLine($"Unexpected match format for Kaufland: {m.Value}");
                        return null;
                    }
                    
                    var quantitySplit = m.Groups[4].Value.Trim().Split(' ');
                    if (quantitySplit.Length != 2)
                    {
                        Debug.WriteLine($"Unexpected quantity format for Kaufland: {m.Groups[4].Value}");
                        return null;
                    }

                    return new Product
                    {
                        ItemName = m.Groups[1].Value.Trim() + " " + m.Groups[2].Value.Trim(),
                        Quantity = quantitySplit[0].Trim(),
                        MeasureQuantity = quantitySplit[1].Trim(),
                        Price = m.Groups[3].Value.Trim() + " Lei",
                        Store = store
                    };
                }
            }).Where(item => item != null).ToList();
        }

        
    }
}
