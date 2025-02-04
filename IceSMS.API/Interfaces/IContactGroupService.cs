using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;

namespace IceSMS.API.Interfaces;

public interface IContactGroupService
{
    Task<List<ContactGroup>> GetGroupsAsync(int tenantId);
    Task<ContactGroup?> GetGroupByIdAsync(int groupId);
    Task<ContactGroup> CreateGroupAsync(int tenantId, ContactGroup group);
    Task<ContactGroup> UpdateGroupAsync(int groupId, ContactGroup group);
    Task<bool> DeleteGroupAsync(int groupId);
    
    // Grup üyeleri
    Task<List<Contact>> GetMembersAsync(int groupId);
    Task<int> GetMemberCountAsync(int groupId);
    
    // Toplu işlemler
    Task<List<Contact>> ImportMembersAsync(int groupId, Stream fileStream, FileType fileType);
    Task<(byte[] FileContents, string ContentType, string FileName)> ExportMembersAsync(
        int groupId, FileType fileType);
} 