using api.Controllers;
using api.Entity;
using api.Extensions;
using api.Helpers;
using AutoMapper;
using fragrancehaven_api.DTOs;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fragrancehaven_api.Controllers
{
    public class TransactionController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;
        private readonly UserManager<AppUser> _userManager;
        public TransactionController(IMapper mapper, IUnitOfWork uow, IPhotoService photoService, UserManager<AppUser> userManager)
        {
            _mapper = mapper;
            _uow = uow;
            _photoService = photoService;
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet] // GET: api/transaction
        public async Task<ActionResult<PagedList<Transaction>>> GetTransactions([FromQuery] PaginationParams paginationParams)
        {
            paginationParams.SearchQuery = paginationParams.SearchQuery.ToLower();
            var transactions = await _uow.transactionRepository.FindAllTransactionsAsync(paginationParams);

            Response.AddPaginationHeader(new PaginationHeader(transactions.CurrentPage,
                transactions.PageSize, transactions.TotalCount, transactions.TotalPages));

            foreach (var transaction in transactions)
            {
                // Set the UserName and UserEmail properties from the related AppUser object
                transaction.UserName = transaction.User?.UserName;
                transaction.UserEmail = transaction.User?.Email;
            }
            return Ok(transactions);
        }

        [Authorize(Policy = "RequireAccount")]
        [HttpGet("{username}")] // GET: api/transaction/{username}
        public async Task<ActionResult<PagedList<Transaction>>> GetUserTransactions(string username, [FromQuery] PaginationParams paginationParams)
        {
            AppUser user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == username.ToLower());
            if (user == null)
                return NotFound("User not found");

            paginationParams.SearchQuery = paginationParams.SearchQuery.ToLower();
            var transactions = await _uow.transactionRepository.FindAllTransactionsForUserAsync(username, paginationParams);

            Response.AddPaginationHeader(new PaginationHeader(transactions.CurrentPage,
                transactions.PageSize, transactions.TotalCount, transactions.TotalPages));

            return Ok(transactions);
        }

        [Authorize(Policy = "RequireAccount")]
        [HttpPost] // POST: api/transaction
        public async Task<ActionResult<PagedList<Transaction>>> PostTransaction(TransactionDTO transactionDTO)
        {
            // Check if a field is missing
            foreach (var property in typeof(TransactionDTO).GetProperties())
            {
                if (property.GetValue(transactionDTO) == null)
                    return BadRequest($"{property.Name} is missing");
            }

            // Find user
            AppUser user = await _userManager.Users.Include(u => u.Cart).SingleOrDefaultAsync(u => u.UserName == transactionDTO.Username.ToLower());
            if (user == null)
                return NotFound("User not found");

            Transaction transaction = _mapper.Map<Transaction>(transactionDTO);
            List<PurchasedProduct> foundProducts = new();
            foreach (var product in transactionDTO.ProductsPurchased)
            {
                Product productFound = await _uow.productRepository.FindProductByName(product.Name);
                if (productFound == null)
                    return NotFound("Product not found");

                // Increase amount sold on product
                productFound.AmountSold += product.Amount;
                // Decrease the amount available
                productFound.Stock -= product.Amount;
                _uow.productRepository.EditProduct(productFound, null);

                foundProducts.Add(new PurchasedProduct
                {
                    Name = productFound.Name,
                    BrandName = productFound.Brand.Name,
                    PurchasedPrice = productFound.SalePrice > 0 ? productFound.SalePrice : productFound.Price,
                    Amount = product.Amount
                });
            }
            transaction.ProductsPurchased.Clear();
            transaction.ProductsPurchased.AddRange(foundProducts);

            // Map foreign keys
            transaction.User = user;
            transaction.UserId = user.Id;

            // Add transaction
            _uow.transactionRepository.AddTransaction(transaction);
            user.Cart.Clear();

            if (await _uow.Complete())
                return Ok(transaction);

            return BadRequest("Problem adding transaction");
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("{transactionId}")] // PUT: api/transaction/{transactionId}
        public async Task<ActionResult<string>> PutRefundTransactions(int transactionId)
        {
            Transaction transactionToRefund  = await _uow.transactionRepository.FindTransactionById(transactionId);
            if (transactionToRefund == null)
                return NotFound("Transaction was not found");

            transactionToRefund.Status = "Refunded";
            transactionToRefund.RefundedDate = DateTime.UtcNow;

            if (await _uow.Complete())
                return Ok("Transaction Refunded");

            return BadRequest("Problem refunding transaction");            
        }
    }
}