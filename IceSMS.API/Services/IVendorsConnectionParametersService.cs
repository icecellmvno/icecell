using IceSMS.API.Models.Domain;

namespace IceSMS.API.Services;

public interface IVendorsConnectionParametersService
{
    Task<IEnumerable<VendorsConnectionParameters>> GetAllAsync();
    Task<VendorsConnectionParameters> GetByIdAsync(int id);
    Task<IEnumerable<VendorsConnectionParameters>> GetByVendorIdAsync(int vendorId);
    Task<VendorsConnectionParameters> CreateAsync(VendorsConnectionParameters parameters);
    Task<VendorsConnectionParameters> UpdateAsync(int id, VendorsConnectionParameters parameters);
    Task<bool> DeleteAsync(int id);
} 