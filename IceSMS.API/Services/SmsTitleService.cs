using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace IceSMS.API.Services;

public class SmsTitleService:ISmsTitleService
    
{
    private readonly ApplicationDbContext _context;

    public SmsTitleService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<SmsTitlesModel>> GetSmsTitlesAsync(int tenantId)
    {
        var titles = await _context.SmsTitles.Where(s => s.TenantId == tenantId).ToListAsync();
        return titles;
    }

    public async Task<SmsTitlesModel> AddSmsTitleAsync(int tenantId,CreateSmsTitleRequest smsTitleRequest)
    {
        var Smstitle = new SmsTitlesModel
        {
            TenantId = tenantId,
            SMSTitle = smsTitleRequest.SMSTitle,
            CreatedAt = DateTime.Now,
            TitleType = smsTitleRequest.TitleType,
            
            

        };
        _context.SmsTitles.Add(Smstitle);
        _context.SaveChanges();
        return Smstitle;
    }

    public async Task<SmsTitlesModel> DeleteSmsTitleAsync(int tenantId, int id)
    {
       var smstitle = _context.SmsTitles.SingleOrDefault(s => s.Id == id && s.TenantId == tenantId);
       _context.SmsTitles.Remove(smstitle);
       _context.SaveChanges();
       return smstitle;
       
    }
}