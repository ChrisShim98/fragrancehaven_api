using System.Text.Json.Serialization;
using api.Entity;

namespace fragrancehaven_api.Entity
{
    public class CartProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public float Price { get; set; }
        public Photo MainPhoto { get; set; }
        public int Amount { get; set; } = 1;
        [JsonIgnore]
        public AppUser User { get; set; }
    }
}