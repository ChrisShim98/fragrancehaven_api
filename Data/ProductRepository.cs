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
                    if (property.Name != "SalePrice")
                    {
                        property.SetValue(product, property.GetValue(updatedProduct));
                    }
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

            if (!string.IsNullOrEmpty(paginationParams.SearchQuery))
            {
                query = query.Where(p => p.Name.ToLower().Contains(paginationParams.SearchQuery));
            }

            if (paginationParams.ProductsWithReview)
                query = query.Where(p => p.Reviews.Any());

            if (paginationParams.ProductsOnSale)
                query = query.Where(p => p.SalePercentage > 0);

            if (paginationParams.ProductsInStock)
                query = query.Where(p => p.Stock > 0);

            query = ApplySorting(query, paginationParams.OrderBy);

            return await PagedList<Product>.CreateAsync(
                query.Include(p => p.Brand).Include(p => p.Photos).Include(p => p.Reviews).AsNoTracking(),
                paginationParams.PageNumber,
                paginationParams.PageSize);
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string orderBy)
        {
            switch (orderBy)
            {
                case "name_asc":
                    query = query.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(p => p.Name);
                    break;
                case "amountSold_asc":
                    query = query.OrderBy(p => p.AmountSold);
                    break;
                case "amountSold_desc":
                    query = query.OrderByDescending(p => p.AmountSold);
                    break;
                // Add other sorting criteria as needed
                default:
                    // Default sorting criteria if orderBy is not provided or invalid
                    query = query.OrderBy(p => p.Id);
                    break;
            }

            return query;
        }
    }
}