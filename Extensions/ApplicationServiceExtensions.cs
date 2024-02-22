using api.Data;
using api.Interfaces;
using api.Services;
using fragrancehaven_api.Data;
using fragrancehaven_api.Helpers;
using fragrancehaven_api.Interfaces;
using fragrancehaven_api.Services;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, 
            IConfiguration config) 
            {
                services.AddCors();
                services.AddScoped<ITokenService, TokenService>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IBrandRepository, BrandRepository>();
                services.AddScoped<IProductRepository, ProductRepository>();
                services.AddScoped<ICartProductRepository, CartProductRepository>();
                services.AddScoped<IPurchasedProductRepository, PurchasedProductRepository>();
                services.AddScoped<IReviewRepository, ReviewRepository>();
                services.AddScoped<ITransactionRepository, TransactionRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IPhotoService, PhotoService>();
                services.Configure<CloudinaryProfile>(config.GetSection("CloudinarySettings"));
                services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

                return services;
            }
    }
}