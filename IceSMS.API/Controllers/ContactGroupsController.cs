using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;
using IceSMS.API.Services;

namespace IceSMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ContactGroupsController : BaseController
{
    private readonly IContactGroupService _groupService;

    public ContactGroupsController(IContactGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ContactGroup>>> GetGroups()
    {
        var groups = await _groupService.GetGroupsAsync(TenantId);
        return Ok(groups);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContactGroup>> GetGroup(int id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        if (group == null)
            return NotFound();

        return Ok(group);
    }

    [HttpPost]
    public async Task<ActionResult<ContactGroup>> CreateGroup(ContactGroup group)
    {
        try
        {
            var createdGroup = await _groupService.CreateGroupAsync(TenantId, group);
            return CreatedAtAction(nameof(GetGroup), new { id = createdGroup.Id }, createdGroup);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ContactGroup>> UpdateGroup(int id, ContactGroup group)
    {
        try
        {
            var updatedGroup = await _groupService.UpdateGroupAsync(id, group);
            return Ok(updatedGroup);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var result = await _groupService.DeleteGroupAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<Contact>>> GetMembers(int id)
    {
        var members = await _groupService.GetMembersAsync(id);
        return Ok(members);
    }

    [HttpGet("{id}/members/count")]
    public async Task<ActionResult<int>> GetMemberCount(int id)
    {
        var count = await _groupService.GetMemberCountAsync(id);
        return Ok(count);
    }

    [HttpPost("{id}/members/import")]
    public async Task<ActionResult<List<Contact>>> ImportMembers(
        int id,
        [FromQuery] FileType fileType,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya yüklenmedi");

        using var stream = file.OpenReadStream();
        try
        {
            var members = await _groupService.ImportMembersAsync(id, stream, fileType);
            return Ok(members);
        }
        catch (Exception ex)
        {
            return BadRequest($"Dosya içe aktarılamadı: {ex.Message}");
        }
    }

    [HttpGet("{id}/members/export")]
    public async Task<IActionResult> ExportMembers(
        int id,
        [FromQuery] FileType fileType)
    {
        try
        {
            var (fileContents, contentType, fileName) = await _groupService.ExportMembersAsync(id, fileType);
            return File(fileContents, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Dışa aktarma başarısız: {ex.Message}");
        }
    }
} 