using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace IceSMS.API.Services;

public class VendorsService : IVendorsService
{
    private readonly ApplicationDbContext _context;

    public VendorsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vendors>> GetAllVendorsAsync()
    {
        return await _context.Vendors.ToListAsync();
    }

    public async Task<Vendors> GetVendorByIdAsync(int id)
    {
        return await _context.Vendors.FindAsync(id);
    }

    public async Task<Vendors> CreateVendorAsync(Vendors vendor)
    {
        vendor.CreatedAt = DateTime.UtcNow;
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();
        return vendor;
    }

    public async Task<Vendors> UpdateVendorAsync(int id, Vendors vendor)
    {
        var existingVendor = await _context.Vendors.FindAsync(id);
        if (existingVendor == null) return null;

        existingVendor.Name = vendor.Name;
        existingVendor.Credits = vendor.Credits;
        existingVendor.IsActive = vendor.IsActive;
        existingVendor.SettingsType = vendor.SettingsType;

        await _context.SaveChangesAsync();
        return existingVendor;
    }

    public async Task<bool> DeleteVendorAsync(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null) return false;

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();
        return true;
    }
} 