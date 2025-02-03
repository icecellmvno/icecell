using IceSMS.API.Models.Domain;

namespace IceSMS.API.Services;

public interface IVendorsService
{
    Task<IEnumerable<Vendors>> GetAllVendorsAsync();
    Task<Vendors?> GetVendorByIdAsync(int id);
    Task<Vendors> CreateVendorAsync(Vendors vendor);
    Task<Vendors?> UpdateVendorAsync(int id, Vendors vendor);
    Task<bool> DeleteVendorAsync(int id);
} 