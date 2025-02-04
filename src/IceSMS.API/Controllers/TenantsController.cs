using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using Microsoft.AspNetCore.Authorization;

namespace IceSMS.API.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class TenantsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public TenantsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Tenant>>> GetTenants()
    {
        var tenants = await _context.Tenants
            .Where(t => t.ParentId == TenantId || t.Id == TenantId)
            .ToListAsync();

        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tenant>> GetTenant(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
            return NotFound();

        return tenant;
    }

    [HttpPost]
    public async Task<ActionResult<Tenant>> CreateTenant(Tenant tenant)
    {
        tenant.ParentId = TenantId;
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.IsActive = true;

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(int id, Tenant tenant)
    {
        if (id != tenant.Id)
            return BadRequest();

        var existingTenant = await _context.Tenants.FindAsync(id);
        if (existingTenant == null)
            return NotFound();

        if (existingTenant.ParentId != TenantId)
            return Forbid();

        existingTenant.Name = tenant.Name;
        existingTenant.Domain = tenant.Domain;
        existingTenant.Description = tenant.Description;
        existingTenant.IsActive = tenant.IsActive;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Tenants.AnyAsync(t => t.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTenant(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return NotFound();

        if (tenant.ParentId != TenantId)
            return Forbid();

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();

        return NoContent();
    }
} 