namespace IceSMS.API.Interfaces;

public interface ISMSService
{
    Task<JsonContent> SendSmsAsync();
}