using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IceSMS.API.Models.Domain;

public class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
} 