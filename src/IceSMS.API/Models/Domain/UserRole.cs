using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IceSMS.API.Models.Domain;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
} 