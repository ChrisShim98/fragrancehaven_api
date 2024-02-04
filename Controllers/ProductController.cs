using api.Controllers;
using api.Extensions;
using api.Helpers;
using AutoMapper;
using fragrancehaven_api.DTOs;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using fragrancehaven_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace fragrancehaven_api.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;
        public ProductController(IMapper mapper, IUnitOfWork uow, IPhotoService photoService)
        {
            _mapper = mapper;
            _uow = uow;
            _photoService = photoService;
        }

        [HttpGet] // GET: api/product/ or /api/product?SearchQuery=TEST
        public async Task<ActionResult<PagedList<Product>>> GetProducts([FromQuery]PaginationParams paginationParams)
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
            if (await _uow.productRepository.CheckIfProductExists(product))
                return BadRequest("Product already exists");

            // Check if brand exists, add if it doesn't
            if (await _uow.brandRepository.CheckIfBrandExists(product.Brand.Name))
            {
                product.Brand = await _uow.brandRepository.GetBrandByName(product.Brand.Name);
            } else {
                await _uow.brandRepository.AddBrand(product.Brand);
                await _uow.Complete(); 
            }      

            // Add product
            _uow.productRepository.AddProduct(product);

            if (await _uow.Complete())
                return Ok("Product saved");

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
        public async Task<ActionResult<string>> EditProduct(int id, [FromBody]ProductDTO productDTO)
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

            // Check if brand exists, add if it doesn't
            if (await _uow.brandRepository.CheckIfBrandExists(productDTO.Brand.Name))
            {
                productDTO.Brand = await _uow.brandRepository.GetBrandByName(productDTO.Brand.Name);
            } else {
                await _uow.brandRepository.AddBrand(productDTO.Brand);
                await _uow.Complete(); 
            } 

            // Set foreign keys on updated product
            var updatedProduct = _mapper.Map<Product>(productDTO);
            updatedProduct.Id = id;
            updatedProduct.BrandId = productDTO.Brand.Id;

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
                photo.isMain = true;

            product.Photos.Add(photo);

            if (await _uow.Complete())
            {
                return Ok("Photo Uploaded");
            };

            return BadRequest("Problem adding photo");
        }

        [HttpDelete("{id}/deletePhoto/{photoId}")] // DELETE: api/product/{id}/deletePhoto/{photoId}
        public async Task<ActionResult<string>> DeletePhoto(int id, int photoId)
        {
            // Find product then image
            Product product = await _uow.productRepository.FindProductById(id);
            var photo = product.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) 
                return NotFound("Image not found");

            if (photo.isMain) 
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
    }
}