using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IceSMS.API.Data;
using IceSMS.API.Models.Domain;

namespace IceSMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PanelSettingsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public PanelSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PanelSettings>>> GetSettings()
    {
        var settings = await _context.PanelSettings
            .Where(s => s.TenantId == TenantId)
            .ToListAsync();

        return Ok(settings);
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<string>> GetSettingValue(string name)
    {
        var setting = await _context.PanelSettings
            .FirstOrDefaultAsync(s => s.TenantId == TenantId && s.Name == name);

        if (setting == null)
            return NotFound();

        return Ok(setting.Value);
    }

    [HttpPost]
    public async Task<ActionResult<PanelSettings>> CreateSetting(PanelSettings setting)
    {
        setting.TenantId = TenantId;

        _context.PanelSettings.Add(setting);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSettingValue), new { name = setting.Name }, setting);
    }

    [HttpPut("{name}")]
    public async Task<IActionResult> UpdateSetting(string name, [FromBody] string value)
    {
        var setting = await _context.PanelSettings
            .FirstOrDefaultAsync(s => s.TenantId == TenantId && s.Name == name);

        if (setting == null)
            return NotFound();

        setting.Value = value;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteSetting(string name)
    {
        var setting = await _context.PanelSettings
            .FirstOrDefaultAsync(s => s.TenantId == TenantId && s.Name == name);

        if (setting == null)
            return NotFound();

        _context.PanelSettings.Remove(setting);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    [AllowAnonymous]
    [HttpGet("tenant")]

    public async Task<ActionResult<Tenant>> GetTenantByDomain(string domain)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Domain == domain&& t.IsActive==true);
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }
} 
