using IceSMS.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IceSMS.API.Models.User;
using IceSMS.API.Services;

namespace IceSMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var users = await _userService.GetUsersAsync(TenantId, page, pageSize);
            return Ok(users);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(new { message = "Kullanıcı silindi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRoles(int id, [FromBody] List<string> roles)
    {
        try
        {
            var result = await _userService.AssignRolesAsync(id, roles);
            if (!result)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(new { message = "Roller atandı" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/roles")]
    public async Task<IActionResult> RevokeRoles(int id, [FromBody] List<string> roles)
    {
        try
        {
            var result = await _userService.RevokeRolesAsync(id, roles);
            if (!result)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(new { message = "Roller kaldırıldı" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var result = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            if (!result)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(new { message = "Şifre değiştirildi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            var result = await _userService.ResetPasswordAsync(id, request.NewPassword);
            if (!result)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(new { message = "Şifre sıfırlandı" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool isActive)
    {
        try
        {
            var result = await _userService.ToggleUserStatusAsync(id, isActive);
            if (!result)
                return NotFound(new { message = "Kullanıcı bulunamadı" });

            return Ok(new { message = isActive ? "Kullanıcı aktifleştirildi" : "Kullanıcı deaktif edildi" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
} 