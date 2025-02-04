using IceSMS.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IceSMS.API.Models.Domain;
using IceSMS.API.Services;

namespace IceSMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class CustomFieldsController : BaseController
{
    private readonly ICustomFieldService _customFieldService;

    public CustomFieldsController(ICustomFieldService customFieldService)
    {
        _customFieldService = customFieldService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomField>>> GetFields()
    {
        var fields = await _customFieldService.GetFieldsAsync(TenantId);
        return Ok(fields);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomField>> GetField(int id)
    {
        var field = await _customFieldService.GetFieldByIdAsync(id);
        if (field == null)
            return NotFound();

        return Ok(field);
    }

    [HttpPost]
    public async Task<ActionResult<CustomField>> CreateField(CustomField field)
    {
        try
        {
            var createdField = await _customFieldService.CreateFieldAsync(TenantId, field);
            return CreatedAtAction(nameof(GetField), new { id = createdField.Id }, createdField);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomField>> UpdateField(int id, CustomField field)
    {
        try
        {
            var updatedField = await _customFieldService.UpdateFieldAsync(id, field);
            return Ok(updatedField);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteField(int id)
    {
        var result = await _customFieldService.DeleteFieldAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}/options")]
    public async Task<ActionResult<List<string>>> GetOptions(int id)
    {
        var options = await _customFieldService.GetOptionsAsync(id);
        return Ok(options);
    }

    [HttpPost("{id}/validate")]
    public async Task<ActionResult<bool>> ValidateValue(int id, [FromBody] string value)
    {
        var field = await _customFieldService.GetFieldByIdAsync(id);
        if (field == null)
            return NotFound();

        var isValid = await _customFieldService.ValidateValueAsync(field, value);
        return Ok(isValid);
    }
} 