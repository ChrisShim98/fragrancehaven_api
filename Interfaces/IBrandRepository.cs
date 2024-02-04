using fragrancehaven_api.Entity;

namespace fragrancehaven_api.Interfaces
{
    public interface IBrandRepository
    {
        Task<Brand> AddBrand(Brand brand);
        Task<bool> CheckIfBrandExists(string brand);
        Task<Brand> GetBrandByName(string brand);

    }
}