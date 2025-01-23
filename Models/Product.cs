using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDS_PROJECT.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string ItemName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }
        public string MeasureUnit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public string Currency {get; set; }

        public string Store { get; set; }
        public string Searched { get; set; }
        public string Link {get; set; }

        public Product() { }
        public Product(Product product)
        {
            ItemName = product.ItemName;
            Quantity = product.Quantity;
            MeasureUnit = product.MeasureUnit;
            Price = product.Price;
            Currency = product.Currency;
            Store = product.Store;
            Searched = product.Searched;
        }

    }
}