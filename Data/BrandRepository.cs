using api.Data;
using AutoMapper;
using fragrancehaven_api.Entity;
using fragrancehaven_api.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace fragrancehaven_api.Data
{
    public class BrandRepository : IBrandRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public BrandRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Brand> AddBrand(Brand brand)
        {
            _context.Brands.Add(brand);
            return await _context.Brands.SingleOrDefaultAsync(b => b.Name == brand.Name);
        }

        public async Task<bool> CheckIfBrandExists(string brand)
        {
            return await _context.Brands.AnyAsync(b => b.Name == brand);
        }

        public async Task<Brand> GetBrandByName(string brand)
        {
            return await _context.Brands.FirstOrDefaultAsync(b => b.Name == brand);
        }
    }
}