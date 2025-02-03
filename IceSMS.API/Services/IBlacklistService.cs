using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;

namespace IceSMS.API.Services;

public interface IBlacklistService
{
    Task<List<Blacklist>> GetBlacklistAsync(int tenantId);
    Task<Blacklist?> GetBlacklistByIdAsync(int id);
    Task<Blacklist> AddToBlacklistAsync(int tenantId, Blacklist blacklist);
    Task<bool> RemoveFromBlacklistAsync(int id);
    Task<bool> IsBlacklistedAsync(int tenantId, string phoneNumber);
    
    // Toplu işlemler
    Task<List<Blacklist>> ImportBlacklistAsync(int tenantId, Stream fileStream, FileType fileType);
    Task<(byte[] FileContents, string ContentType, string FileName)> ExportBlacklistAsync(
        int tenantId, FileType fileType);
    
    // Validasyon
    Task<bool> IsPhoneNumberValidAsync(string phoneNumber);
    Task<string> FormatPhoneNumberAsync(string phoneNumber);
} 