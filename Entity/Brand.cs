using System.Text.Json.Serialization;

namespace fragrancehaven_api.Entity
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public List<Product> Products { get; set; } = new();
    }
}