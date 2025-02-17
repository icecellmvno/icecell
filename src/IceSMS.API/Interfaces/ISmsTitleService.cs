using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Settings;

namespace IceSMS.API.Interfaces;

public interface ISmsTitleService
{
    Task<List<SmsTitlesModel>> GetSmsTitlesAsync(int tenantId);
    Task<SmsTitlesModel> AddSmsTitleAsync(int tenantId,CreateSmsTitleRequest smsTitlesModel);
    Task<SmsTitlesModel?> DeleteSmsTitleAsync(int tenantId, int id);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<SmsTitlesModel>> GetAllAsync();
}