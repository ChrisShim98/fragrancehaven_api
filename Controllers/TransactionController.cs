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

            return Ok(transactions);
        }

        [Authorize(Policy = "RequireAccount")]
        [HttpGet("{username}")] // GET: api/transaction/{username}
        public async Task<ActionResult<PagedList<Transaction>>> GetForUserTransactions(string username, [FromQuery] PaginationParams paginationParams)
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

            Transaction transaction = _mapper.Map<Transaction>(transactionDTO);
            List<Product> foundProducts = new();
            foreach (var product in transactionDTO.ProductsPurchased)
            {
                Product productFound = await _uow.productRepository.FindProductById(product.Id);
                if (productFound == null)
                    return NotFound("Product not found");

                // Increase amount sold on product
                productFound.AmountSold += product.Amount;
                _uow.productRepository.EditProduct(productFound, null);

                foundProducts.Add(productFound);
            }
            transaction.ProductsPurchased.Clear();
            transaction.ProductsPurchased.AddRange(foundProducts);
            
            // Find user
            AppUser user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == transactionDTO.Username.ToLower());
            if (user == null)
                return NotFound("User not found");

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
    }
}