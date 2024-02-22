using System.Text.Json.Serialization;
using api.Entity;

namespace fragrancehaven_api.Entity
{
    public class Transaction
    {
        public int Id { get; set; }
        public List<PurchasedProduct> ProductsPurchased { get; set; }
        public float TotalSpent { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public AppUser User { get; set; }
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;
        public int LastFourOfCard { get; set; }
        public string Status { get; set; } = "Paid";
    }
}