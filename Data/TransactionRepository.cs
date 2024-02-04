using api.Data;
using AutoMapper;
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

        public async Task<IEnumerable<Transaction>> GetAllTransactions()
        {
            return await _context.Transactions.Select(t => t).ToListAsync();
        }
    }
}