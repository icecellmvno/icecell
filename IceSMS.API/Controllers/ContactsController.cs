using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;
using IceSMS.API.Services;

namespace IceSMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ContactsController : BaseController
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Contact>>> GetContacts([FromQuery] int? groupId = null)
    {
        var contacts = await _contactService.GetContactsAsync(TenantId, groupId);
        return Ok(contacts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Contact>> GetContact(int id)
    {
        var contact = await _contactService.GetContactByIdAsync(id);
        if (contact == null)
            return NotFound();

        return Ok(contact);
    }

    [HttpPost]
    public async Task<ActionResult<Contact>> CreateContact(Contact contact)
    {
        try
        {
            var createdContact = await _contactService.CreateContactAsync(TenantId, contact);
            return CreatedAtAction(nameof(GetContact), new { id = createdContact.Id }, createdContact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Contact>> UpdateContact(int id, Contact contact)
    {
        try
        {
            var updatedContact = await _contactService.UpdateContactAsync(id, contact);
            return Ok(updatedContact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var result = await _contactService.DeleteContactAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/groups/{groupId}")]
    public async Task<IActionResult> AddToGroup(int id, int groupId)
    {
        var result = await _contactService.AddToGroupAsync(id, groupId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}/groups/{groupId}")]
    public async Task<IActionResult> RemoveFromGroup(int id, int groupId)
    {
        var result = await _contactService.RemoveFromGroupAsync(id, groupId);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}/groups")]
    public async Task<ActionResult<List<ContactGroup>>> GetGroups(int id)
    {
        var groups = await _contactService.GetGroupsAsync(id);
        return Ok(groups);
    }

    [HttpPost("import")]
    public async Task<ActionResult<List<Contact>>> ImportContacts(
        [FromQuery] FileType fileType,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya yüklenmedi");

        using var stream = file.OpenReadStream();
        try
        {
            var contacts = await _contactService.ImportContactsAsync(TenantId, stream, fileType);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            return BadRequest($"Dosya içe aktarılamadı: {ex.Message}");
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportContacts(
        [FromQuery] FileType fileType,
        [FromQuery] int? groupId = null)
    {
        try
        {
            var (fileContents, contentType, fileName) = await _contactService.ExportContactsAsync(TenantId, fileType, groupId);
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
        var isValid = await _contactService.IsPhoneNumberValidAsync(phoneNumber);
        return Ok(isValid);
    }
} 