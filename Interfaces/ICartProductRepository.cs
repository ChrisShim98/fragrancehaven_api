using fragrancehaven_api.Entity;

namespace fragrancehaven_api.Interfaces
{
    public interface ICartProductRepository
    {
        void AddProduct(CartProduct product);
        void DeleteProduct(CartProduct product);
        Task<List<CartProduct>> GetAllProductsAsync();
    }
}