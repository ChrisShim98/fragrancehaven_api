using api.Controllers;
using api.Entity;
using api.Extensions;
using api.Helpers;
using AutoMapper;
using fragrancehaven_api.DTOs;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fragrancehaven_api.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;
        private readonly UserManager<AppUser> _userManager;
        public ProductController(IMapper mapper, IUnitOfWork uow, IPhotoService photoService, UserManager<AppUser> userManager)
        {
            _mapper = mapper;
            _uow = uow;
            _photoService = photoService;
            _userManager = userManager;
        }

        [HttpGet] // GET: api/product/ or /api/product?SearchQuery=TEST
        public async Task<ActionResult<PagedList<Product>>> GetProducts([FromQuery] PaginationParams paginationParams)
        {
            paginationParams.SearchQuery = paginationParams.SearchQuery.ToLower();
            var products = await _uow.productRepository.GetAllProductsAsync(paginationParams);

            Response.AddPaginationHeader(new PaginationHeader(products.CurrentPage,
                products.PageSize, products.TotalCount, products.TotalPages));

            return Ok(products);
        }

        [HttpPost] // POST: api/product/
        public async Task<ActionResult<string>> CreateProduct(ProductDTO productDTO)
        {
            // Check if a field is missing
            foreach (var property in typeof(ProductDTO).GetProperties())
            {
                if (property.GetValue(productDTO) == null)
                    return BadRequest($"{property.Name} is missing");
            }

            // Check if product already exists
            var product = _mapper.Map<Product>(productDTO);
            product.Brand.Name = productDTO.BrandName;

            if (await _uow.productRepository.CheckIfProductExists(product))
                return BadRequest("Product already exists");

            // Check if brand exists, add if it doesn't
            if (await _uow.brandRepository.CheckIfBrandExists(product.Brand.Name))
            {
                Brand foundBrand = await _uow.brandRepository.GetBrandByName(product.Brand.Name);
                product.BrandId = foundBrand.Id;
                product.Brand = foundBrand;
            }
            else
            {
                await _uow.brandRepository.AddBrand(product.Brand);
                await _uow.Complete();
            }

            // Add product
            _uow.productRepository.AddProduct(product);

            if (await _uow.Complete())
                return Ok("Product saved. Id: " + product.Id);

            return BadRequest("Failed to save product");
        }

        [HttpDelete("{id}")] // DELETE: api/product/{id}
        public async Task<ActionResult<string>> DeleteProduct(int id)
        {
            // Check if product Exists
            Product product = await _uow.productRepository.FindProductById(id);
            if (product == null)
                return NotFound("Product cannot be found");

            // Delete Product
            _uow.productRepository.DeleteProduct(product);

            if (await _uow.Complete())
                return Ok("Product deleted");

            return BadRequest("Failed to save product");
        }

        [HttpPut("{id}")] // PUT: api/product/{id}
        public async Task<ActionResult<string>> EditProduct(int id, [FromBody] ProductDTO productDTO)
        {
            // Check if a field is missing
            foreach (var property in typeof(ProductDTO).GetProperties())
            {
                if (property.GetValue(productDTO) == null)
                    return BadRequest($"{property.Name} is missing");
            }

            // Check if product exists
            Product product = await _uow.productRepository.FindProductById(id);
            if (product == null)
                return NotFound("Product does not exist");

            // Set foreign keys on updated product
            var updatedProduct = _mapper.Map<Product>(productDTO);
            updatedProduct.Id = id;
            updatedProduct.Brand.Name = productDTO.BrandName;
            updatedProduct.Photos = product.Photos;

            if (product.Name != productDTO.Name)
            {
                if (await _uow.productRepository.CheckIfProductExists(updatedProduct))
                    return BadRequest("Product already exists");
            }

            // Check if brand exists, add if it doesn't
            if (await _uow.brandRepository.CheckIfBrandExists(updatedProduct.Brand.Name))
            {
                Brand foundBrand = await _uow.brandRepository.GetBrandByName(updatedProduct.Brand.Name);
                updatedProduct.BrandId = foundBrand.Id;
                updatedProduct.Brand = foundBrand;
            }
            else
            {
                await _uow.brandRepository.AddBrand(updatedProduct.Brand);
                await _uow.Complete();
            }

            // Update Product
            _uow.productRepository.EditProduct(product, updatedProduct);

            if (await _uow.Complete())
                return Ok("Product updated");

            return BadRequest("Failed to save product");
        }

        [HttpPost("{id}/addPhoto")] // POST: api/product/{id}/addPhoto
        public async Task<ActionResult<string>> AddProductPhoto(int id, IFormFile file)
        {
            // Find product
            Product product = await _uow.productRepository.FindProductById(id);

            // Add Image to product
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if (product.Photos.Count == 0)
                photo.IsMain = true;

            product.Photos.Add(photo);

            if (await _uow.Complete())
                return Ok("Photo Uploaded");

            return BadRequest("Problem adding photo");
        }

        [HttpDelete("{id}/deletePhoto/{photoId}")] // DELETE: api/product/{id}/deletePhoto/{photoId}
        public async Task<ActionResult<string>> DeletePhoto(int id, int photoId)
        {
            // Find product then image
            Product product = await _uow.productRepository.FindProductById(id);
            if (product == null)
                return NotFound("Product not found");

            Photo photo = new();
            if (product.Photos.Count - 1 >= photoId)
            {
                photo = product.Photos[photoId];
            }
            else
            {
                return NotFound("Image not found");
            }

            if (photo == null)
                return NotFound("Image not found");

            if (photo.IsMain)
                return BadRequest("You cannot delete product main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            product.Photos.Remove(photo);

            if (await _uow.Complete())
                return Ok("Image deleted");

            return BadRequest("Problem deleting photo");
        }

        [HttpPut("{id}/mainPhoto/{photoId}")] // PUT: api/product/{id}/mainPhoto/{photoId}
        public async Task<ActionResult<string>> SetMainPhoto(int id, int photoId)
        {
            // Find product then image
            Product product = await _uow.productRepository.FindProductById(id);
            if (product == null)
                return NotFound("Product not found");

            Photo photo = new();
            if (product.Photos.Count - 1 >= photoId)
            {
                photo = product.Photos[photoId];
            }
            else
            {
                return NotFound("Image not found");
            }

            if (photo == null)
                return NotFound("Image not found");

            // Set all photo IsMain to false
            foreach (Photo currentPhoto in product.Photos)
            {
                currentPhoto.IsMain = false;
            }

            // Set the new photo as the main photo
            product.Photos[photoId].IsMain = true;

            // Update Product
            _uow.productRepository.EditProduct(product, null);

            if (await _uow.Complete())
                return Ok("Main photo set");

            return BadRequest("Problem setting main photo");
        }

        [HttpPost("{id}/addReview")] // POST: api/product/{id}/addReview
        public async Task<ActionResult<string>> AddReview(int id, [FromBody] ReviewDTO review)
        {
            // Find product then add review
            Product product = await _uow.productRepository.FindProductById(id);
            if (product == null)
                return NotFound("Product not found");

            AppUser user = await _userManager.Users.SingleOrDefaultAsync(u => u.UserName == review.ReviewerName);
            if (user == null)
                return NotFound("User not found");

            Review productReview = new Review
            {
                Message = review.Message,
                Rating = review.Rating,
                ReviewerId = user.Id,
                ReviewerName = review.ReviewerName,
                Reviewer = user,
                ProductId = product.Id,
                Product = product
            };

            product.Reviews.Add(productReview);

            if (await _uow.Complete())
                return Ok("Review Added");

            return BadRequest("Problem adding review");
        }
    }
}