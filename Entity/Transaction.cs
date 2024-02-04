using api.Entity;

namespace fragrancehaven_api.Entity
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;
    }
}