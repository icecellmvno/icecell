using IceSMS.API.Interfaces;
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

    [HttpGet("get")]
    public async Task<IActionResult> GetBlacklist([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null)
    {
        try 
        {
            var (blacklist, totalCount, totalPages) = await _blacklistService.GetBlacklistAsync(TenantId, page, limit, search);
            return Ok(new { 
                data = blacklist,
                total = totalCount,
                total_page = totalPages,
                page = page,
                limit = limit
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Blacklist>> GetBlacklistById(int id)
    {
        var blacklist = await _blacklistService.GetBlacklistByIdAsync(id);
        if (blacklist == null)
            return NotFound(new { message = "Kayıt bulunamadı" });

        return Ok(blacklist);
    }

    [HttpPost("add")]
    public async Task<ActionResult<Blacklist>> AddToBlacklist([FromBody] Blacklist blacklist)
    {
        try
        {
            blacklist.tenant_id = TenantId;
            var addedBlacklist = await _blacklistService.AddToBlacklistAsync(blacklist);
            return Ok(new { message = "Kayıt başarıyla eklendi", data = addedBlacklist });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("bulk-add")]
    public async Task<ActionResult<List<Blacklist>>> BulkAddToBlacklist([FromBody] List<string> phones)
    {
        try
        {
            var blacklists = phones.Select(phone => new Blacklist { phone = phone, tenant_id = TenantId }).ToList();
            var addedBlacklists = await _blacklistService.BulkAddToBlacklistAsync(blacklists);
            return Ok(new { message = "Kayıtlar başarıyla eklendi", data = addedBlacklists });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> RemoveFromBlacklist(int id)
    {
        try
        {
            var result = await _blacklistService.RemoveFromBlacklistAsync(id);
            if (!result)
                return NotFound(new { message = "Kayıt bulunamadı" });

            return Ok(new { message = "Kayıt başarıyla silindi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkRemoveFromBlacklist([FromBody] List<int> ids)
    {
        try
        {
            var result = await _blacklistService.BulkRemoveFromBlacklistAsync(ids);
            return Ok(new { message = $"{result} kayıt başarıyla silindi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("check/{phoneNumber}")]
    public async Task<ActionResult<bool>> IsBlacklisted(string phoneNumber)
    {
        var isBlacklisted = await _blacklistService.IsBlacklistedAsync(TenantId, phoneNumber);
        return Ok(new { isBlacklisted });
    }

    [HttpPost("import")]
    public async Task<ActionResult<List<Blacklist>>> ImportBlacklist(
        [FromQuery] FileType fileType,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya yüklenmedi" });

        using var stream = file.OpenReadStream();
        try
        {
            var blacklists = await _blacklistService.ImportBlacklistAsync(TenantId, stream, fileType);
            return Ok(new { message = "Dosya başarıyla içe aktarıldı", data = blacklists });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Dosya içe aktarılamadı: {ex.Message}" });
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportBlacklist([FromQuery] FileType fileType = FileType.Csv)
    {
        try
        {
            var (fileContents, contentType, fileName) = await _blacklistService.ExportBlacklistAsync(TenantId, fileType);
            return File(fileContents, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Dışa aktarma başarısız: {ex.Message}" });
        }
    }
} 