using api.Data;
using AutoMapper;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace fragrancehaven_api.Data
{
    public class CartProductRepository : ICartProductRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public CartProductRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public void AddProduct(CartProduct product)
        {
            _context.CartProducts.Add(product);
        }

        public void DeleteProduct(CartProduct product)
        {
            _context.CartProducts.Remove(product);
        }

        public async Task<List<CartProduct>> GetAllProductsAsync()
        {
            return await _context.CartProducts.ToListAsync();
        }
    }
}