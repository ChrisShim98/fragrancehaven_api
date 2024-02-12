using System.Text.Json;
using api.Entity;
using fragrancehaven_api.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class SeedData
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await File.ReadAllTextAsync("wwwroot/UserSeedData.json");

            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            var roles = new List<AppRole>
            {
                new AppRole{Name = "Customer"},
                new AppRole{Name = "Admin"},
            };

            foreach (var role in roles) 
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Customer");
            }

            var admin = new AppUser
            {
                UserName = "admin",
                Email = "chris@gmail.com"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] {"Admin"});
        }
        public static async Task SeedProducts(DataContext _dataContext)
        {
            if (_dataContext.Products.Any()) return;

            var productData = await File.ReadAllTextAsync("wwwroot/ProductSeedData.json");

            var products = JsonSerializer.Deserialize<List<Product>>(productData);

            foreach (var product in products)
            {
                foreach (var review in product.Reviews)
                {
                    review.DateAdded = DateTime.SpecifyKind(review.DateAdded, DateTimeKind.Utc);
                }
                _dataContext.Products.AddRange(product);
                _dataContext.SaveChanges();
            };
        }
    }
}