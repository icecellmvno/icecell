using System.ComponentModel.DataAnnotations;
using IceSMS.API.Models.Enums;

namespace IceSMS.API.Models.Settings;

public class CreateSmsTitleRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(11)]
    public string SMSTitle { get; set; }

    [Required(ErrorMessage = "Titletype is required")]
    public SmsTitleType TitleType { get; set; }
}