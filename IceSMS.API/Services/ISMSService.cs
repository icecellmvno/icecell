using IceSMS.API.Models.Domain;

namespace IceSMS.API.Services;

public interface ISMSService
{
    Task<JsonContent> SendSmsAsync();
}