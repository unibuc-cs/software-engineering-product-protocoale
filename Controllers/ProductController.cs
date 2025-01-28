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
                ViewBag.AuchanResults = utils.FilterItems(existingProducts, quantityNumber, unit, "auchan");
                ViewBag.MegaResults = utils.FilterItems(existingProducts, quantityNumber, unit, "mega");
                return View("Index");
            }

            // Execute the search scripts for Carrefour and Kaufland
            var carrefourTask = utils.StartSearchScript("Carrefour.py", query, exactItemName);
            var auchanTask = utils.StartSearchScript("Auchan.py", query, exactItemName);
            var megaTask = utils.StartSearchScript("Mega.py", query, exactItemName);

            await Task.WhenAll(carrefourTask, auchanTask);

            var carrefourResults = utils.ParseResults(carrefourTask.Result, "carrefour");
            var auchanResults = utils.ParseResults(auchanTask.Result, "auchan");
            var megaResults = utils.ParseResults(megaTask.Result, "mega");

            ViewBag.CarrefourResults = utils.FilterItems(carrefourResults, quantityNumber, unit);
            ViewBag.AuchanResults = utils.FilterItems(auchanResults, quantityNumber, unit);
            ViewBag.MegaResults = utils.FilterItems(megaResults, quantityNumber, unit);

            await utils.SaveToDatabase(carrefourResults, query);
            await utils.SaveToDatabase(auchanResults, query);
            await utils.SaveToDatabase(megaResults, query);

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
    }
}
