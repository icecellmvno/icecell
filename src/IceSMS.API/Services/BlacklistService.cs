using IceSMS.API.Data;
using IceSMS.API.Interfaces;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace IceSMS.API.Services;

public class BlacklistService : IBlacklistService
{
    private readonly ApplicationDbContext _context;

    public BlacklistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Blacklist> Blacklist, int TotalCount, int TotalPages)> GetBlacklistAsync(int tenantId, int page, int limit, string? search)
    {
        var query = _context.Blacklist
            .Where(b => b.tenant_id == tenantId && !b.DeletedAt.HasValue);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => b.phone.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)limit);

        var blacklist = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (blacklist, totalCount, totalPages);
    }

    public async Task<Blacklist?> GetBlacklistByIdAsync(int id)
    {
        return await _context.Blacklist
            .FirstOrDefaultAsync(b => b.ID == id && !b.DeletedAt.HasValue);
    }

    public async Task<Blacklist> AddToBlacklistAsync(Blacklist blacklist)
    {
        // Telefon numarasını formatla
        blacklist.phone = await FormatPhoneNumberAsync(blacklist.phone);

        // Telefon numarası geçerli mi kontrol et
        if (!await IsPhoneNumberValidAsync(blacklist.phone))
        {
            throw new InvalidOperationException("Geçersiz telefon numarası");
        }

        // Telefon numarası zaten var mı kontrol et
        var exists = await _context.Blacklist
            .AnyAsync(b => b.phone == blacklist.phone && b.tenant_id == blacklist.tenant_id && !b.DeletedAt.HasValue);

        if (exists)
        {
            throw new InvalidOperationException("Bu telefon numarası zaten kara listede");
        }

        blacklist.CreatedAt = DateTime.UtcNow;
        blacklist.UpdatedAt = DateTime.UtcNow;

        _context.Blacklist.Add(blacklist);
        await _context.SaveChangesAsync();

        return blacklist;
    }

    public async Task<List<Blacklist>> BulkAddToBlacklistAsync(List<Blacklist> blacklists)
    {
        var addedBlacklists = new List<Blacklist>();

        foreach (var blacklist in blacklists)
        {
            try
            {
                var added = await AddToBlacklistAsync(blacklist);
                addedBlacklists.Add(added);
            }
            catch
            {
                // Hatalı kayıtları atla
                continue;
            }
        }

        return addedBlacklists;
    }

    public async Task<bool> RemoveFromBlacklistAsync(int id)
    {
        var blacklist = await _context.Blacklist.FindAsync(id);
        if (blacklist == null || blacklist.DeletedAt.HasValue)
            return false;

        blacklist.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> BulkRemoveFromBlacklistAsync(List<int> ids)
    {
        var blacklists = await _context.Blacklist
            .Where(b => ids.Contains(b.ID) && !b.DeletedAt.HasValue)
            .ToListAsync();

        foreach (var blacklist in blacklists)
        {
            blacklist.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return blacklists.Count;
    }

    public async Task<bool> IsBlacklistedAsync(int tenantId, string phoneNumber)
    {
        var formattedPhone = await FormatPhoneNumberAsync(phoneNumber);
        return await _context.Blacklist
            .AnyAsync(b => b.phone == formattedPhone && b.tenant_id == tenantId && !b.DeletedAt.HasValue);
    }

    public async Task<List<Blacklist>> ImportBlacklistAsync(int tenantId, Stream fileStream, FileType fileType)
    {
        var addedBlacklists = new List<Blacklist>();

        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<dynamic>();
        foreach (var record in records)
        {
            var phone = record.phone?.ToString() ?? record.Phone?.ToString();
            if (string.IsNullOrEmpty(phone)) continue;

            try
            {
                var blacklist = new Blacklist
                {
                    phone = phone,
                    tenant_id = tenantId
                };

                var added = await AddToBlacklistAsync(blacklist);
                addedBlacklists.Add(added);
            }
            catch
            {
                // Hatalı kayıtları atla
                continue;
            }
        }

        return addedBlacklists;
    }

    public async Task<(byte[] FileContents, string ContentType, string FileName)> ExportBlacklistAsync(int tenantId, FileType fileType)
    {
        var blacklist = await _context.Blacklist
            .Where(b => b.tenant_id == tenantId && !b.DeletedAt.HasValue)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(blacklist);
        await writer.FlushAsync();

        var bytes = memoryStream.ToArray();
        var fileName = $"karalist_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        return (bytes, "text/csv", fileName);
    }

    public async Task<bool> IsPhoneNumberValidAsync(string phoneNumber)
    {
        // Basit bir telefon numarası doğrulama
        // Gerçek uygulamada daha kapsamlı bir doğrulama yapılabilir
        return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length >= 10;
    }

    public async Task<string> FormatPhoneNumberAsync(string phoneNumber)
    {
        // Telefon numarasını temizle (sadece rakamları al)
        var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Başında 0 varsa kaldır
        if (cleaned.StartsWith("0"))
        {
            cleaned = cleaned.Substring(1);
        }

        // Başında ülke kodu yoksa ekle
        if (!cleaned.StartsWith("90"))
        {
            cleaned = "90" + cleaned;
        }

        return cleaned;
    }
} 