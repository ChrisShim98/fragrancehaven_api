using api.Data;
using api.Helpers;
using AutoMapper;
using fragrancehaven_api.DTOs;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace fragrancehaven_api.Data
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public TransactionRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
        }

        public async Task<PagedList<Transaction>> FindAllTransactionsAsync(PaginationParams paginationParams)
        {
            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(paginationParams.SearchQuery))
            {
                query = query.Where(t => t.UserName.ToLower().Contains(paginationParams.SearchQuery) || t.Id.ToString().Contains(paginationParams.SearchQuery));
            }

            return await PagedList<Transaction>.CreateAsync(
                query.Include(t => t.ProductsPurchased).Include(t => t.User).OrderByDescending(t => t.DatePurchased.Date).AsNoTracking(),
                paginationParams.PageNumber,
                paginationParams.PageSize);
        }

        public async Task<List<Transaction>> FindAllTransactionsAnalyticsAsync(DateFilter dateFilter, bool refunded)
        {
            var query = refunded 
                ? _context.Transactions.Where(t => t.RefundedDate >= dateFilter.StartDate && t.RefundedDate <= dateFilter.EndDate).Include(t => t.ProductsPurchased).Include(t => t.User).AsNoTracking()
                : _context.Transactions.Where(t => t.DatePurchased >= dateFilter.StartDate && t.DatePurchased <= dateFilter.EndDate).Include(t => t.ProductsPurchased).Include(t => t.User).AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<PagedList<Transaction>> FindAllTransactionsForUserAsync(string username, PaginationParams paginationParams)
        {
            var query = _context.Transactions.AsQueryable();

            query = query.Where(t => t.User.UserName.ToLower() == username.ToLower());

            return await PagedList<Transaction>.CreateAsync(
                query.Include(t => t.ProductsPurchased).OrderByDescending(t => t.DatePurchased.Date).AsNoTracking(),
                paginationParams.PageNumber,
                paginationParams.PageSize);
        }

        public async Task<Transaction> FindTransactionById(int id)
        {
            return await _context.Transactions.Include(t => t.ProductsPurchased).SingleOrDefaultAsync(t => t.Id == id);
        }
    }
}