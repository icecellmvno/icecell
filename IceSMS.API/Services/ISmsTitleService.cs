using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Settings;

namespace IceSMS.API.Services;

public interface ISmsTitleService
{
    Task<List<SmsTitlesModel>> GetSmsTitlesAsync(int tenantId);
    Task<SmsTitlesModel> AddSmsTitleAsync(int tenantId,CreateSmsTitleRequest smsTitlesModel);
    Task<SmsTitlesModel> DeleteSmsTitleAsync(int tenantId, int id);
    
}