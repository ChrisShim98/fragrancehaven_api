namespace fragrancehaven_api.Interfaces
{
    public interface IUnitOfWork
    {
        IProductRepository productRepository { get; }
        IBrandRepository brandRepository { get; }
        IReviewRepository reviewRepository { get; }
        ITransactionRepository transactionRepository { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}