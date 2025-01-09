namespace MDS_PROJECT.Models
{
    public class CartItem
    {
        public string ItemName { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
        public string CarrefourMessage { get; set; }
        public string KauflandMessage { get; set; }
        public string CarrefourStoreItemName { get; set; }
        public string KauflandStoreItemName { get; set; }
        public int Multiplier { get; set; } = 1; // Default to 1 if not specified
    }
}
