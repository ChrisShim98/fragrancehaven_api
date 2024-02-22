using api.Data;
using AutoMapper;
using fragrancehaven_api.Interfaces;

namespace fragrancehaven_api.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UnitOfWork(DataContext context, IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }
        public IProductRepository productRepository => new ProductRepository(_context, _mapper);

        public IBrandRepository brandRepository => new BrandRepository(_context, _mapper);

        public IReviewRepository reviewRepository => new ReviewRepository(_context, _mapper);

        public ITransactionRepository transactionRepository => new TransactionRepository(_context, _mapper);

        public ICartProductRepository cartProductRepository => new CartProductRepository(_context, _mapper);


        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}