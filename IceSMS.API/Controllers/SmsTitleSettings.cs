using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Settings;
using IceSMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IceSMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class SmsTitleSettings : BaseController
{
    private readonly ISmsTitleService _smsTitleService;

    public SmsTitleSettings(ISmsTitleService smsTitleService)
    {
        _smsTitleService = smsTitleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSmsTitles()
    {
        var titles = await _smsTitleService.GetAllAsync();
        return Ok(titles);
    }

    [HttpPost]
    public async Task<ActionResult<SmsTitlesModel>> CreateSmsTitle(CreateSmsTitleRequest createSmsTitleRequest)
    {
        try
        {
            var createdTitle = await _smsTitleService.AddSmsTitleAsync(TenantId, createSmsTitleRequest);
            return CreatedAtAction(nameof(GetSmsTitles), createdTitle);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    public async Task<ActionResult<SmsTitlesModel>> DeleteSmsTitle(int id)
    {
        try
        {
            var title = await _smsTitleService.DeleteSmsTitleAsync(TenantId, id);
            if (title == null)
            {
                return NotFound();
            }
            return Ok(title);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }
}