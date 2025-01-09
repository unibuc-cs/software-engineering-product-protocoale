using MDS_PROJECT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDS_PROJECT.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products {  get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<FavoriteProduct> FavoriteItems { get; set; }

    }
}