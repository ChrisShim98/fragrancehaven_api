using api.DTOs;
using api.Entity;
using AutoMapper;
using fragrancehaven_api.DTOs;
using fragrancehaven_api.Entity;

namespace api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<RegisterDTO, AppUser>();
            CreateMap<AppUser, AppUserDTO>();
            CreateMap<ProductDTO, Product>();
            CreateMap<TransactionDTO, Transaction>();
            CreateMap<PhotoDTO, Photo>();
            CreateMap<ReviewDTO, Review>();
        }
    }
}