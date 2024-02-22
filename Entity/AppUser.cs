using System.Text.Json.Serialization;
using fragrancehaven_api.Entity;
using Microsoft.AspNetCore.Identity;

namespace api.Entity
{
    public class AppUser : IdentityUser<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
        [JsonIgnore]
        public List<CartProduct> Cart { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();
    }
}