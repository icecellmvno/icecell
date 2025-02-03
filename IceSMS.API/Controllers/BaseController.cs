using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IceSMS.API.Controllers;

public class BaseController : ControllerBase
{
    protected int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    protected string UserEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    protected int TenantId => int.Parse(User.FindFirstValue("tenant_id") ?? "0");
    
    protected int? CurrentTenantId => HttpContext.Items["TenantId"] as int?;
    
    protected bool HasTenant => CurrentTenantId.HasValue;
    
    protected IActionResult TenantNotFound()
    {
        return BadRequest("Geçerli bir tenant bulunamadı");
    }
} 