using api.Entity;

namespace fragrancehaven_api.Entity
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Scent { get; set; }
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        public float Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public List<Photo> Photos { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public int Amount { get; set; } = 1;
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}