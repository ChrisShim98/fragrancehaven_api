using api.Helpers;
using fragrancehaven_api.Entity;

namespace fragrancehaven_api.Interfaces
{
    public interface IProductRepository
    {
        void AddProduct(Product product);
        void DeleteProduct(Product product);
        Task<Product> FindProductById(int productId);
        Task<PagedList<Product>> GetAllProductsAsync(PaginationParams paginationParams);
        Task<bool> CheckIfProductExists(Product product);
        void EditProduct(Product product, Product updatedProduct);
    }
}