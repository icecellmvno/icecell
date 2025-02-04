using IceSMS.API.Models.Domain;

namespace IceSMS.API.Interfaces;

public interface IVendorsActionParametersService
{
    Task<IEnumerable<VendorsActionParameters>> GetAllAsync();
    Task<VendorsActionParameters?> GetByIdAsync(int id);
    Task<IEnumerable<VendorsActionParameters>> GetByVendorIdAsync(int vendorId);
    Task<VendorsActionParameters> CreateAsync(VendorsActionParameters parameters);
    Task<VendorsActionParameters?> UpdateAsync(int id, VendorsActionParameters parameters);
    Task<bool> DeleteAsync(int id);
} 