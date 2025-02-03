using IceSMS.API.Models.Enums;

namespace IceSMS.API.Models.Domain;

public class Vendors
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }
    public Boolean IsActive { get; set; }
    public VendorsConnectionSettingsType SettingsType { get; set; }
}

public class VendorsConnectionParameters
{
    public int id { get; set; }
    public int VendorId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public DateTime CreatedAt { get; set; }
    public Boolean IsActive { get; set; }
}

public class VendorsActionParameters
{
    public int id { get; set; }
    public int VendorId { get; set; }
    public int VendorConnectionParamtersId { get; set; }
    public string Action { get; set; }
    public string Template{get;set;}
    public string Method { get; set; }
}
