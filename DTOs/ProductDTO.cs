using fragrancehaven_api.Entity;

namespace fragrancehaven_api.DTOs
{
    public class ProductDTO
    {
        public string Name { get; set; }
        public string Scent { get; set; }

        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public float Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; } = 1;
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}