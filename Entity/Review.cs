using api.Entity;

namespace fragrancehaven_api.Entity
{
    public class Review
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int Rating { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int ReviewerId { get; set; }
        public AppUser Reviewer { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}