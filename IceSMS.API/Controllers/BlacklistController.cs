using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;
using IceSMS.API.Services;

namespace IceSMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class BlacklistController : BaseController
{
    private readonly IBlacklistService _blacklistService;

    public BlacklistController(IBlacklistService blacklistService)
    {
        _blacklistService = blacklistService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Blacklist>>> GetBlacklist()
    {
        var blacklist = await _blacklistService.GetBlacklistAsync(TenantId);
        return Ok(blacklist);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Blacklist>> GetBlacklistById(int id)
    {
        var blacklist = await _blacklistService.GetBlacklistByIdAsync(id);
        if (blacklist == null)
            return NotFound();

        return Ok(blacklist);
    }

    [HttpPost]
    public async Task<ActionResult<Blacklist>> AddToBlacklist(Blacklist blacklist)
    {
        try
        {
            var addedBlacklist = await _blacklistService.AddToBlacklistAsync(TenantId, blacklist);
            return CreatedAtAction(nameof(GetBlacklistById), new { id = addedBlacklist.Id }, addedBlacklist);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromBlacklist(int id)
    {
        var result = await _blacklistService.RemoveFromBlacklistAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("check/{phoneNumber}")]
    public async Task<ActionResult<bool>> IsBlacklisted(string phoneNumber)
    {
        var isBlacklisted = await _blacklistService.IsBlacklistedAsync(TenantId, phoneNumber);
        return Ok(isBlacklisted);
    }

    [HttpPost("import")]
    public async Task<ActionResult<List<Blacklist>>> ImportBlacklist(
        [FromQuery] FileType fileType,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya yüklenmedi");

        using var stream = file.OpenReadStream();
        try
        {
            var blacklists = await _blacklistService.ImportBlacklistAsync(TenantId, stream, fileType);
            return Ok(blacklists);
        }
        catch (Exception ex)
        {
            return BadRequest($"Dosya içe aktarılamadı: {ex.Message}");
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportBlacklist([FromQuery] FileType fileType)
    {
        try
        {
            var (fileContents, contentType, fileName) = await _blacklistService.ExportBlacklistAsync(TenantId, fileType);
            return File(fileContents, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Dışa aktarma başarısız: {ex.Message}");
        }
    }

    [HttpPost("validate-phone")]
    public async Task<ActionResult<bool>> ValidatePhoneNumber([FromBody] string phoneNumber)
    {
        var isValid = await _blacklistService.IsPhoneNumberValidAsync(phoneNumber);
        return Ok(isValid);
    }
} 