using fragrancehaven_api.Entity;
using Microsoft.AspNetCore.Identity;

namespace api.Entity
{
    public class AppUser : IdentityUser<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
        public List<Product> Cart { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();
    }
}