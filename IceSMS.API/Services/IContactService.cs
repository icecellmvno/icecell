using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;

namespace IceSMS.API.Services;

public interface IContactService
{
    Task<List<Contact>> GetContactsAsync(int tenantId, int? groupId = null);
    Task<Contact?> GetContactByIdAsync(int contactId);
    Task<Contact> CreateContactAsync(int tenantId, Contact contact);
    Task<Contact> UpdateContactAsync(int contactId, Contact contact);
    Task<bool> DeleteContactAsync(int contactId);
    
    // Grup işlemleri
    Task<bool> AddToGroupAsync(int contactId, int groupId);
    Task<bool> RemoveFromGroupAsync(int contactId, int groupId);
    Task<List<ContactGroup>> GetGroupsAsync(int contactId);
    
    // Toplu işlemler
    Task<List<Contact>> ImportContactsAsync(int tenantId, Stream fileStream, FileType fileType);
    Task<(byte[] FileContents, string ContentType, string FileName)> ExportContactsAsync(
        int tenantId, FileType fileType, int? groupId = null);
    
    // Validasyon
    Task<bool> IsPhoneNumberValidAsync(string phoneNumber);
    Task<string> FormatPhoneNumberAsync(string phoneNumber);
    Task<bool> IsPhoneNumberUniqueAsync(int tenantId, string phoneNumber, int? excludeContactId = null);
} 