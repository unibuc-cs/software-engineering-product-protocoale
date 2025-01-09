using MDS_PROJECT.Data;
using MDS_PROJECT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MDS_PROJECT.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new favorite
        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteProduct favorite)
        {
            if (string.IsNullOrWhiteSpace(favorite.Name) || string.IsNullOrWhiteSpace(favorite.Quantity) || string.IsNullOrWhiteSpace(favorite.Unit))
            {
                Console.WriteLine($"Name: {favorite.Name}, Quantity: {favorite.Quantity}, Unit: {favorite.Unit}");
                return BadRequest("All fields are required.");
            }

            favorite.AddedDate = DateTime.Now; // Set the added date to now

            // Add to the database
            _context.FavoriteItems.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(favorite); // Return the added favorite item as a response
        }

        // Get all favorites
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var favorites = await _context.FavoriteItems.ToListAsync();
            return Json(favorites); // Return the list of favorite items as JSON
        }

        // Optionally: Delete a favorite
        [HttpPost]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var favorite = await _context.FavoriteItems.FindAsync(id);
            if (favorite == null)
            {
                return NotFound();
            }

            _context.FavoriteItems.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
