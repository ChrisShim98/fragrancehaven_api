using api.Data;
using api.Helpers;
using AutoMapper;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace fragrancehaven_api.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public ProductRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
        }

        public async Task<bool> CheckIfProductExists(Product product)
        {
            return await _context.Products.AnyAsync(p => p.Name == product.Name);
        }

        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
        }

        public void EditProduct(Product product, Product updatedProduct)
        {
            if (updatedProduct != null)
            {
                foreach (var property in typeof(Product).GetProperties())
                {
                    property.SetValue(product, property.GetValue(updatedProduct));
                }
            }

            _context.Products.Update(product);
        }

        public async Task<Product> FindProductById(int productId)
        {
            return await _context.Products.Include(p => p.Brand).Include(p => p.Photos).Include(p => p.Reviews).SingleOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<PagedList<Product>> GetAllProductsAsync(PaginationParams paginationParams)
        {
            var query = _context.Products.AsQueryable();

            if (paginationParams.SearchQuery != "")
            {
                query = query.Where(p => p.Name.ToLower().Contains(paginationParams.SearchQuery));
            }

            query = query.OrderBy(u => u.Id).Include(p => p.Brand).Include(p => p.Photos).Include(p => p.Reviews);

            return await PagedList<Product>.CreateAsync(
                query.AsNoTracking(),
                paginationParams.PageNumber,
                paginationParams.PageSize);
        }
    }
}