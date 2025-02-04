using System.Text;
using System.Xml.Serialization;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Newtonsoft.Json;
using IceSMS.API.Data;
using IceSMS.API.Interfaces;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;

namespace IceSMS.API.Services;

public class ContactGroupService : IContactGroupService
{
    private readonly ApplicationDbContext _context;
    private readonly IContactService _contactService;

    public ContactGroupService(ApplicationDbContext context, IContactService contactService)
    {
        _context = context;
        _contactService = contactService;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<List<ContactGroup>> GetGroupsAsync(int tenantId)
    {
        return await _context.ContactGroups
            .Include(g => g.Contacts)
            .Where(g => g.TenantId == tenantId)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<ContactGroup?> GetGroupByIdAsync(int groupId)
    {
        return await _context.ContactGroups
            .Include(g => g.Contacts)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<ContactGroup> CreateGroupAsync(int tenantId, ContactGroup group)
    {
        if (await _context.ContactGroups.AnyAsync(g => g.Name == group.Name && g.TenantId == tenantId))
            throw new InvalidOperationException("Bu grup adı zaten kullanılıyor");

        group.TenantId = tenantId;
        group.CreatedAt = DateTime.UtcNow;

        _context.ContactGroups.Add(group);
        await _context.SaveChangesAsync();

        return group;
    }

    public async Task<ContactGroup> UpdateGroupAsync(int groupId, ContactGroup group)
    {
        var existingGroup = await _context.ContactGroups.FindAsync(groupId);
        if (existingGroup == null)
            throw new InvalidOperationException("Grup bulunamadı");

        if (group.Name != existingGroup.Name)
        {
            if (await _context.ContactGroups.AnyAsync(g => g.Name == group.Name && g.TenantId == existingGroup.TenantId))
                throw new InvalidOperationException("Bu grup adı zaten kullanılıyor");
        }

        existingGroup.Name = group.Name;
        existingGroup.Description = group.Description;
        existingGroup.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingGroup;
    }

    public async Task<bool> DeleteGroupAsync(int groupId)
    {
        var group = await _context.ContactGroups.FindAsync(groupId);
        if (group == null)
            return false;

        _context.ContactGroups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Contact>> GetMembersAsync(int groupId)
    {
        var group = await _context.ContactGroups
            .Include(g => g.Contacts)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        return group?.Contacts.ToList() ?? new List<Contact>();
    }

    public async Task<int> GetMemberCountAsync(int groupId)
    {
        return await _context.ContactGroups
            .Where(g => g.Id == groupId)
            .Select(g => g.Contacts.Count)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Contact>> ImportMembersAsync(int groupId, Stream fileStream, FileType fileType)
    {
        var group = await _context.ContactGroups
            .Include(g => g.Contacts)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new InvalidOperationException("Grup bulunamadı");

        var contacts = fileType switch
        {
            FileType.Csv => await ImportFromCsvAsync(fileStream),
            FileType.Excel => await ImportFromExcelAsync(fileStream),
            FileType.Json => await ImportFromJsonAsync(fileStream),
            FileType.Xml => await ImportFromXmlAsync(fileStream),
            _ => throw new ArgumentException("Desteklenmeyen dosya formatı")
        };

        var importedContacts = new List<Contact>();
        foreach (var contact in contacts)
        {
            try
            {
                contact.TenantId = group.TenantId;
                var importedContact = await _contactService.CreateContactAsync(group.TenantId, contact);
                group.Contacts.Add(importedContact);
                importedContacts.Add(importedContact);
            }
            catch
            {
                continue;
            }
        }

        await _context.SaveChangesAsync();
        return importedContacts;
    }

    public async Task<(byte[] FileContents, string ContentType, string FileName)> ExportMembersAsync(
        int groupId, FileType fileType)
    {
        var contacts = await GetMembersAsync(groupId);
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        return fileType switch
        {
            FileType.Csv => (
                await ExportToCsvAsync(contacts),
                "text/csv",
                $"group_members_{timestamp}.csv"
            ),
            FileType.Excel => (
                await ExportToExcelAsync(contacts),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"group_members_{timestamp}.xlsx"
            ),
            FileType.Json => (
                await ExportToJsonAsync(contacts),
                "application/json",
                $"group_members_{timestamp}.json"
            ),
            FileType.Xml => (
                await ExportToXmlAsync(contacts),
                "application/xml",
                $"group_members_{timestamp}.xml"
            ),
            _ => throw new ArgumentException("Desteklenmeyen dosya formatı")
        };
    }

    private async Task<List<Contact>> ImportFromCsvAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<Contact>().ToList();
    }

    private async Task<List<Contact>> ImportFromExcelAsync(Stream stream)
    {
        var contacts = new List<Contact>();
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
            contacts.Add(new Contact
            {
                FirstName = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                LastName = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                PhoneNumber = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                Notes = worksheet.Cells[row, 4].Value?.ToString()
            });
        }

        return contacts;
    }

    private async Task<List<Contact>> ImportFromJsonAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<List<Contact>>(json) ?? new List<Contact>();
    }

    private async Task<List<Contact>> ImportFromXmlAsync(Stream stream)
    {
        var serializer = new XmlSerializer(typeof(List<Contact>));
        return (List<Contact>)serializer.Deserialize(stream)!;
    }

    private async Task<byte[]> ExportToCsvAsync(List<Contact> contacts)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(contacts);
        await writer.FlushAsync();
        return memoryStream.ToArray();
    }

    private async Task<byte[]> ExportToExcelAsync(List<Contact> contacts)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Contacts");

        // Başlıklar
        worksheet.Cells[1, 1].Value = "Ad";
        worksheet.Cells[1, 2].Value = "Soyad";
        worksheet.Cells[1, 3].Value = "Telefon";
        worksheet.Cells[1, 4].Value = "Notlar";

        // Veriler
        for (int i = 0; i < contacts.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = contacts[i].FirstName;
            worksheet.Cells[i + 2, 2].Value = contacts[i].LastName;
            worksheet.Cells[i + 2, 3].Value = contacts[i].PhoneNumber;
            worksheet.Cells[i + 2, 4].Value = contacts[i].Notes;
        }

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    private async Task<byte[]> ExportToJsonAsync(List<Contact> contacts)
    {
        var json = JsonConvert.SerializeObject(contacts, Formatting.Indented);
        return Encoding.UTF8.GetBytes(json);
    }

    private async Task<byte[]> ExportToXmlAsync(List<Contact> contacts)
    {
        var serializer = new XmlSerializer(typeof(List<Contact>));
        using var memoryStream = new MemoryStream();
        serializer.Serialize(memoryStream, contacts);
        return memoryStream.ToArray();
    }

    public async Task<bool> ValidateGroupAsync(int groupId)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> ValidateGroupsAsync(List<int> groupIds)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> ValidateGroupNameAsync(string groupName)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> ValidateGroupNamesAsync(List<string> groupNames)
    {
        return await Task.FromResult(true);
    }

    public async Task<bool> ValidateGroupFileAsync(string filePath)
    {
        return await Task.FromResult(true);
    }
} 