using IceSMS.API.Models.Enums;

namespace IceSMS.API.Models.Domain;



public class SmsTitlesModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public required string SMSTitle { get; set; }
    public SmsTitleType TitleType { get; set; }

    public Tenant? Parent { get; set; }
    public DateTime CreatedAt { get; set; }
    
    
}