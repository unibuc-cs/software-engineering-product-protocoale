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
using Microsoft.EntityFrameworkCore;

namespace MDS_PROJECT.Controllers
{
    // Controller for handling product-related actions
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db; // Database context
        private readonly Utilities utils;

        // Constructor to initialize the controller with the necessary dependencies
        public ProductController(
            ApplicationDbContext context,
            Utilities _utils)
        {
            db = context;
            utils = _utils;
        }

        // Action to handle the search request for products in both stores
        [HttpPost]
        public async Task<IActionResult> Search(string query, string quantity, string unit, bool exactItemName)
        {
            var quantityNumber = utils.StringToDecimal(quantity);

            // Return an empty view if the query is empty
            if (string.IsNullOrEmpty(query))
            {
                return View("Index");
            }

            // Check if the product is already in the database
            var existingProducts = db.Products.Where(p => p.Searched == query).ToList();
            if (existingProducts.Any())
            {
                ViewBag.CarrefourResults = utils.FilterItems(existingProducts, quantityNumber, unit, "carrefour");
                ViewBag.KauflandResults = utils.FilterItems(existingProducts, quantityNumber, unit, "kaufland");
                return View("Index");
            }

            // Execute the search scripts for Carrefour and Kaufland
            var carrefourTask = utils.StartSearchScript("Carrefour.py", query, exactItemName);
            var kauflandTask = utils.StartSearchScript("Kaufland.py", query, exactItemName);

            await Task.WhenAll(carrefourTask, kauflandTask);

            var carrefourResults = utils.ParseResults(carrefourTask.Result, "carrefour");
            var kauflandResults = utils.ParseKauflandResults(kauflandTask.Result);

            ViewBag.CarrefourResults = utils.FilterItems(carrefourResults, quantityNumber, unit);
            ViewBag.KauflandResults = utils.FilterItems(kauflandResults, quantityNumber, unit);

            await utils.SaveToDatabase(carrefourResults, query);
            await utils.SaveToDatabase(kauflandResults, query);

            return View("Index");

        } // Search

        // Action to display the initial search view
        public IActionResult Index()
        {
            return View();
        }

        // Action to handle errors
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Add a new favorite
    //     [HttpPost]
    //     public async Task<IActionResult> AddFavorite([FromBody] FavoriteProduct favorite)
    //     {
    //         if (string.IsNullOrWhiteSpace(favorite.Name) || string.IsNullOrWhiteSpace(favorite.Quantity) || string.IsNullOrWhiteSpace(favorite.Unit))
    //         {
    //             Console.WriteLine($"Name: {favorite.Name}, Quantity: {favorite.Quantity}, Unit: {favorite.Unit}");
    //             return BadRequest("All fields are required.");
    //         }

    //         favorite.AddedDate = DateTime.Now; // Set the added date to now

    //         // Add to the database
    //         db.FavoriteItems.Add(favorite);
    //         await db.SaveChangesAsync();

    //         return Ok(favorite); // Return the added favorite item as a response
    //     }

    //     // Get all favorites
    //     [HttpGet]
    //     public async Task<IActionResult> GetFavorites()
    //     {
    //         var favorites = await db.FavoriteItems.ToListAsync();
    //         return Json(favorites); // Return the list of favorite items as JSON
    //     }

    //     // Optionally: Delete a favorite
    //     [HttpPost]
    //     public async Task<IActionResult> DeleteFavorite(int id)
    //     {
    //         var favorite = await db.FavoriteItems.FindAsync(id);
    //         if (favorite == null)
    //         {
    //             return NotFound();
    //         }

    //         db.FavoriteItems.Remove(favorite);
    //         await db.SaveChangesAsync();

    //         return Ok();
    //     }
    }
}
