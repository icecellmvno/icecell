using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace IceSMS.API.Services;

public class VendorsConnectionParametersService : IVendorsConnectionParametersService
{
    private readonly ApplicationDbContext _context;

    public VendorsConnectionParametersService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VendorsConnectionParameters>> GetAllAsync()
    {
        return await _context.VendorsConnectionParameters.ToListAsync();
    }

    public async Task<VendorsConnectionParameters> GetByIdAsync(int id)
    {
        return await _context.VendorsConnectionParameters.FindAsync(id);
    }

    public async Task<IEnumerable<VendorsConnectionParameters>> GetByVendorIdAsync(int vendorId)
    {
        return await _context.VendorsConnectionParameters
            .Where(x => x.VendorId == vendorId)
            .ToListAsync();
    }

    public async Task<VendorsConnectionParameters> CreateAsync(VendorsConnectionParameters parameters)
    {
        parameters.CreatedAt = DateTime.UtcNow;
        _context.VendorsConnectionParameters.Add(parameters);
        await _context.SaveChangesAsync();
        return parameters;
    }

    public async Task<VendorsConnectionParameters> UpdateAsync(int id, VendorsConnectionParameters parameters)
    {
        var existing = await _context.VendorsConnectionParameters.FindAsync(id);
        if (existing == null) return null;

        existing.Username = parameters.Username;
        existing.Password = parameters.Password;
        existing.Port = parameters.Port;
        existing.IsActive = parameters.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var parameters = await _context.VendorsConnectionParameters.FindAsync(id);
        if (parameters == null) return false;

        _context.VendorsConnectionParameters.Remove(parameters);
        await _context.SaveChangesAsync();
        return true;
    }
} 