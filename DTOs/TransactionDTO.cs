using fragrancehaven_api.Entity;

namespace fragrancehaven_api.DTOs
{
    public class TransactionDTO
    {
        public List<Product> ProductsPurchased { get; set; }
        public float TotalSpent { get; set; }
        public string Username { get; set; }
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;
        public int LastFourOfCard { get; set; }
        public string Status { get; set; } = "Paid";
    }
}