using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;

namespace IceSMS.API.Interfaces;

public interface IBlacklistService
{
    Task<(List<Blacklist> Blacklist, int TotalCount, int TotalPages)> GetBlacklistAsync(int tenantId, int page, int limit, string? search);
    Task<Blacklist?> GetBlacklistByIdAsync(int id);
    Task<Blacklist> AddToBlacklistAsync(Blacklist blacklist);
    Task<List<Blacklist>> BulkAddToBlacklistAsync(List<Blacklist> blacklists);
    Task<bool> RemoveFromBlacklistAsync(int id);
    Task<int> BulkRemoveFromBlacklistAsync(List<int> ids);
    Task<bool> IsBlacklistedAsync(int tenantId, string phoneNumber);
    
    // Toplu işlemler
    Task<List<Blacklist>> ImportBlacklistAsync(int tenantId, Stream fileStream, FileType fileType);
    Task<(byte[] FileContents, string ContentType, string FileName)> ExportBlacklistAsync(
        int tenantId, FileType fileType);
    
    // Validasyon
    Task<bool> IsPhoneNumberValidAsync(string phoneNumber);
    Task<string> FormatPhoneNumberAsync(string phoneNumber);
} 