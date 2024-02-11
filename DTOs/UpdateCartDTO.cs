using fragrancehaven_api.Entity;

namespace fragrancehaven_api.DTOs
{
    public class UpdateCartDTO
    {
        public string Username { get; set; }
        public List<Product> Cart { get; set; }
    }
}