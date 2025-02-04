using IceSMS.API.Data;
using IceSMS.API.Interfaces;
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

    public async Task<VendorsConnectionParameters?> GetByIdAsync(int id)
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

    public async Task<VendorsConnectionParameters?> UpdateAsync(int id, VendorsConnectionParameters parameters)
    {
        var existingParameters = await _context.VendorsConnectionParameters.FindAsync(id);
        if (existingParameters == null) return null;

        existingParameters.Username = parameters.Username;
        existingParameters.Password = parameters.Password;
        existingParameters.Port = parameters.Port;
        existingParameters.IsActive = parameters.IsActive;

        await _context.SaveChangesAsync();
        return existingParameters;
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