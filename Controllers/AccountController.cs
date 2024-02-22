using api.DTOs;
using api.Entity;
using api.Interfaces;
using AutoMapper;
using fragrancehaven_api.DTOs;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper, IUnitOfWork uow)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _userManager = userManager;
            _uow = uow;
        }

        [HttpPost("register")] // POST: api/account/register
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username))
                return BadRequest("Username is taken!");

            if (await EmailExists(registerDTO.Email))
                return BadRequest("Email is taken!");

            var user = _mapper.Map<AppUser>(registerDTO);

            user.UserName = registerDTO.Username.ToLower();
            user.Email = registerDTO.Email.ToLower();

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDTO
            {
                Username = user.UserName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user),
            };
        }

        public async Task<bool> UserExists(string username)
        {
            return await Task.FromResult(_userManager.Users.Any(x => x.UserName == username.ToLower()));
        }
        public async Task<bool> EmailExists(string email)
        {
            return await Task.FromResult(_userManager.Users.Any(x => x.Email == email.ToLower()));
        }

        [HttpPost("login")] // POST: api/account/login
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await Task.FromResult(_userManager.Users
                .SingleOrDefault(x => x.UserName == loginDTO.Username.ToLower()));

            if (user == null) return Unauthorized("Invalid username");

            var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!result) return Unauthorized("Invalid Password");

            return new UserDTO
            {
                Username = user.UserName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user),
            };
        }

        [Authorize(Policy = "RequireAccount")]
        [HttpPost("updatePassword")] // POST: api/account/updatePassword
        public async Task<ActionResult<UserDTO>> UpdatePassword(PasswordResetDTO passwordResetDTO)
        {
            var user = await Task.FromResult(_userManager.Users
                .SingleOrDefault(x => x.UserName == passwordResetDTO.Username.ToLower()));

            if (user == null) return Unauthorized("Invalid Username");

            if (passwordResetDTO.NewPassword != passwordResetDTO.ConfirmPassword)
                return Unauthorized("Confirm password should match new password");

            if (passwordResetDTO.CurrentPassword == passwordResetDTO.NewPassword)
                return Unauthorized("New password and current password should not be the same");

            var result = await _userManager.CheckPasswordAsync(user, passwordResetDTO.CurrentPassword);

            if (!result) return Unauthorized("Invalid Password");

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, passwordResetDTO.CurrentPassword, passwordResetDTO.NewPassword);

            if (!changePasswordResult.Succeeded)
                return BadRequest("Problem updating password");

            return new UserDTO
            {
                Username = user.UserName,
                Email = user.Email,
                Token = await _tokenService.CreateToken(user),
            };
        }

        [Authorize(Policy = "RequireAccount")]
        [HttpGet("cart")] // GET: api/account/cart
        public async Task<ActionResult<List<Product>>> GetCart([FromQuery] string username)
        {
            AppUser user = await _userManager.Users.Include(u => u.Cart).ThenInclude(p => p.MainPhoto).SingleOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("User not found");

            return Ok(user.Cart);
        }

        [Authorize(Policy = "RequireAccount")]
        [HttpDelete("cart")] // DELETE: api/account/cart
        public async Task<ActionResult<List<Product>>> EmptyCart([FromQuery] string username)
        {
            AppUser user = await _userManager.Users
                .Include(u => u.Cart)
                .SingleOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                return NotFound("User not found");

            // Clear the existing cart
            user.Cart.Clear();

            // Save changes to the database
            await _uow.Complete();

            return Ok(user.Cart);
        }

        [Authorize(Policy = "RequireAccount")]
        [HttpPut("cart/{productName}")] // PUT: api/account/cart/{productName}
        public async Task<ActionResult<List<Product>>> ModifyProductInCart(string productName, [FromQuery] string username, bool addProduct)
        {
            AppUser user = await _userManager.Users
                .Include(u => u.Cart)
                .SingleOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                return NotFound("User not found");

            Product foundProduct = await _uow.productRepository.FindProductByName(productName.Replace("%20", " "));
            if (foundProduct == null)
                return NotFound("Product not found");

            if (addProduct)
            {   
                CartProduct productToAdd = user.Cart.FirstOrDefault(p => p.Name == foundProduct.Name);

                if (productToAdd != null && productToAdd.Amount + 1 > foundProduct.Stock)
                    return BadRequest("Insufficient items in stock");

                if (productToAdd == null)
                {
                    productToAdd = new CartProduct
                    {
                        Name = foundProduct.Name,
                        BrandName = foundProduct.Brand.Name,
                        Price = foundProduct.SalePrice > 0 ? foundProduct.SalePrice : foundProduct.Price,
                        MainPhoto = foundProduct.Photos.Single(p => p.IsMain == true),
                        User = user
                    };
                    user.Cart.Add(productToAdd);
                }
                else
                {
                    productToAdd.Amount += 1;
                }
            }
            else
            {
                CartProduct productToRemove = user.Cart.FirstOrDefault(p => p.Name == foundProduct.Name);

                if (productToRemove == null)
                    return NotFound("Product not found");

                if (productToRemove.Amount == 1)
                {
                    user.Cart.Remove(productToRemove);
                    _uow.cartProductRepository.DeleteProduct(productToRemove);
                }
                else
                {
                    productToRemove.Amount -= 1;
                }
            }

            // Save changes to the database
            await _uow.Complete();

            return Ok(user.Cart);
        }
    }
}