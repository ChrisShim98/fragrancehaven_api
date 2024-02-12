using api.Helpers;
using fragrancehaven_api.Entity;

namespace fragrancehaven_api.Interfaces
{
    public interface ITransactionRepository
    {
        void AddTransaction(Transaction transaction);
        Task<Transaction> FindTransactionById(int id);
        Task<PagedList<Transaction>> FindAllTransactionsAsync(PaginationParams paginationParams);
        Task<PagedList<Transaction>> FindAllTransactionsForUserAsync(string username, PaginationParams paginationParams);
    }
}