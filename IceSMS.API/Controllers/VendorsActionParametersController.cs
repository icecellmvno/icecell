using IceSMS.API.Interfaces;
using IceSMS.API.Models.Domain;
using IceSMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IceSMS.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorsActionParametersController : ControllerBase
{
    private readonly IVendorsActionParametersService _service;

    public VendorsActionParametersController(IVendorsActionParametersService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorsActionParameters>>> GetAll()
    {
        var parameters = await _service.GetAllAsync();
        return Ok(parameters);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VendorsActionParameters>> GetById(int id)
    {
        var parameter = await _service.GetByIdAsync(id);
        if (parameter == null) return NotFound();
        return Ok(parameter);
    }

    [HttpGet("vendor/{vendorId}")]
    public async Task<ActionResult<IEnumerable<VendorsActionParameters>>> GetByVendorId(int vendorId)
    {
        var parameters = await _service.GetByVendorIdAsync(vendorId);
        return Ok(parameters);
    }

    [HttpPost]
    public async Task<ActionResult<VendorsActionParameters>> Create(VendorsActionParameters parameters)
    {
        var created = await _service.CreateAsync(parameters);
        return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VendorsActionParameters>> Update(int id, VendorsActionParameters parameters)
    {
        var updated = await _service.UpdateAsync(id, parameters);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
} 