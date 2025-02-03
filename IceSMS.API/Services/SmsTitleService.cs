using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace IceSMS.API.Services;

public class SmsTitleService : ISmsTitleService
{
    private readonly ApplicationDbContext _context;

    public SmsTitleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SmsTitlesModel>> GetSmsTitlesAsync(int tenantId)
    {
        return await _context.SmsTitles.Where(s => s.TenantId == tenantId).ToListAsync();
    }

    public async Task<SmsTitlesModel> AddSmsTitleAsync(int tenantId, CreateSmsTitleRequest smsTitleRequest)
    {
        var smsTitle = new SmsTitlesModel
        {
            TenantId = tenantId,
            SMSTitle = smsTitleRequest.SMSTitle,
            CreatedAt = DateTime.Now,
            TitleType = smsTitleRequest.TitleType
        };
        
        await _context.SmsTitles.AddAsync(smsTitle);
        await _context.SaveChangesAsync();
        return smsTitle;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var title = await _context.SmsTitles.FindAsync(id);
        if (title == null) return false;

        _context.SmsTitles.Remove(title);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SmsTitlesModel?> DeleteSmsTitleAsync(int tenantId, int id)
    {
        var title = await _context.SmsTitles.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
        if (title == null) return null;

        _context.SmsTitles.Remove(title);
        await _context.SaveChangesAsync();
        return title;
    }

    public async Task<IEnumerable<SmsTitlesModel>> GetAllAsync()
    {
        return await _context.SmsTitles.ToListAsync();
    }
}