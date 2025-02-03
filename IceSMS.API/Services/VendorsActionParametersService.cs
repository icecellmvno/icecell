using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace IceSMS.API.Services;

public class VendorsActionParametersService : IVendorsActionParametersService
{
    private readonly ApplicationDbContext _context;

    public VendorsActionParametersService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VendorsActionParameters>> GetAllAsync()
    {
        return await _context.VendorsActionParameters.ToListAsync();
    }

    public async Task<VendorsActionParameters> GetByIdAsync(int id)
    {
        return await _context.VendorsActionParameters.FindAsync(id);
    }

    public async Task<IEnumerable<VendorsActionParameters>> GetByVendorIdAsync(int vendorId)
    {
        return await _context.VendorsActionParameters
            .Where(x => x.VendorId == vendorId)
            .ToListAsync();
    }

    public async Task<VendorsActionParameters> CreateAsync(VendorsActionParameters parameters)
    {
        _context.VendorsActionParameters.Add(parameters);
        await _context.SaveChangesAsync();
        return parameters;
    }

    public async Task<VendorsActionParameters> UpdateAsync(int id, VendorsActionParameters parameters)
    {
        var existing = await _context.VendorsActionParameters.FindAsync(id);
        if (existing == null) return null;

        existing.Action = parameters.Action;
        existing.Template = parameters.Template;
        existing.Method = parameters.Method;
        existing.VendorConnectionParamtersId = parameters.VendorConnectionParamtersId;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var parameters = await _context.VendorsActionParameters.FindAsync(id);
        if (parameters == null) return false;

        _context.VendorsActionParameters.Remove(parameters);
        await _context.SaveChangesAsync();
        return true;
    }
} 