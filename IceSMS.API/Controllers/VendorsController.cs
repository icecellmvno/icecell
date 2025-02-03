using IceSMS.API.Models.Domain;
using IceSMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IceSMS.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly IVendorsService _vendorsService;

    public VendorsController(IVendorsService vendorsService)
    {
        _vendorsService = vendorsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vendors>>> GetVendors()
    {
        var vendors = await _vendorsService.GetAllVendorsAsync();
        return Ok(vendors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Vendors>> GetVendor(int id)
    {
        var vendor = await _vendorsService.GetVendorByIdAsync(id);
        if (vendor == null) return NotFound();
        return Ok(vendor);
    }

    [HttpPost]
    public async Task<ActionResult<Vendors>> CreateVendor(Vendors vendor)
    {
        var createdVendor = await _vendorsService.CreateVendorAsync(vendor);
        return CreatedAtAction(nameof(GetVendor), new { id = createdVendor.Id }, createdVendor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(int id, Vendors vendor)
    {
        var updatedVendor = await _vendorsService.UpdateVendorAsync(id, vendor);
        if (updatedVendor == null) return NotFound();
        return Ok(updatedVendor);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        var result = await _vendorsService.DeleteVendorAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
} 