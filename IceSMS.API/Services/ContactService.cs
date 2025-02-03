using System.Text;
using System.Xml.Serialization;
using CsvHelper;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PhoneNumbers;
using OfficeOpenXml;
using Newtonsoft.Json;
using IceSMS.API.Data;
using IceSMS.API.Models.Domain;
using IceSMS.API.Models.Enums;

namespace IceSMS.API.Services;

public class ContactService : IContactService
{
    private readonly ApplicationDbContext _context;
    private readonly PhoneNumberUtil _phoneUtil;

    public ContactService(ApplicationDbContext context)
    {
        _context = context;
        _phoneUtil = PhoneNumberUtil.GetInstance();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<List<Contact>> GetContactsAsync(int tenantId, int? groupId = null)
    {
        var query = _context.Contacts
            .Include(c => c.Groups)
            .Include(c => c.CustomFields)
            .ThenInclude(f => f.CustomField)
            .Where(c => c.TenantId == tenantId);

        if (groupId.HasValue)
        {
            query = query.Where(c => c.Groups.Any(g => g.Id == groupId));
        }

        return await query.ToListAsync();
    }

    public async Task<Contact?> GetContactByIdAsync(int contactId)
    {
        return await _context.Contacts
            .Include(c => c.Groups)
            .Include(c => c.CustomFields)
            .ThenInclude(f => f.CustomField)
            .FirstOrDefaultAsync(c => c.Id == contactId);
    }

    public async Task<Contact> CreateContactAsync(int tenantId, Contact contact)
    {
        // Telefon numarasını doğrula ve formatla
        if (!await IsPhoneNumberValidAsync(contact.PhoneNumber))
            throw new InvalidOperationException("Geçersiz telefon numarası");

        contact.PhoneNumber = await FormatPhoneNumberAsync(contact.PhoneNumber);

        // Numaranın benzersiz olduğunu kontrol et
        if (!await IsPhoneNumberUniqueAsync(tenantId, contact.PhoneNumber))
            throw new InvalidOperationException("Bu telefon numarası zaten kayıtlı");

        contact.TenantId = tenantId;
        contact.CreatedAt = DateTime.UtcNow;

        // UPSERT işlemi
        var sql = @"
            INSERT INTO ""Contacts"" (""FirstName"", ""LastName"", ""PhoneNumber"", ""Notes"", ""CreatedAt"", ""TenantId"")
            VALUES (@FirstName, @LastName, @PhoneNumber, @Notes, @CreatedAt, @TenantId)
            ON CONFLICT (""PhoneNumber"", ""TenantId"") 
            DO UPDATE SET 
                ""FirstName"" = EXCLUDED.""FirstName"",
                ""LastName"" = EXCLUDED.""LastName"",
                ""Notes"" = EXCLUDED.""Notes"",
                ""UpdatedAt"" = NOW()
            RETURNING *;";

        var parameters = new
        {
            contact.FirstName,
            contact.LastName,
            contact.PhoneNumber,
            contact.Notes,
            contact.CreatedAt,
            contact.TenantId
        };

        var result = await _context.Contacts.FromSqlRaw(sql, parameters).FirstOrDefaultAsync();
        return result ?? contact;
    }

    public async Task<Contact> UpdateContactAsync(int contactId, Contact contact)
    {
        var existingContact = await _context.Contacts.FindAsync(contactId);
        if (existingContact == null)
            throw new InvalidOperationException("Kişi bulunamadı");

        // Telefon numarası değişiyorsa kontrol et
        if (contact.PhoneNumber != existingContact.PhoneNumber)
        {
            if (!await IsPhoneNumberValidAsync(contact.PhoneNumber))
                throw new InvalidOperationException("Geçersiz telefon numarası");

            contact.PhoneNumber = await FormatPhoneNumberAsync(contact.PhoneNumber);

            if (!await IsPhoneNumberUniqueAsync(existingContact.TenantId, contact.PhoneNumber, contactId))
                throw new InvalidOperationException("Bu telefon numarası zaten kayıtlı");
        }

        existingContact.FirstName = contact.FirstName;
        existingContact.LastName = contact.LastName;
        existingContact.PhoneNumber = contact.PhoneNumber;
        existingContact.Notes = contact.Notes;
        existingContact.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingContact;
    }

    public async Task<bool> DeleteContactAsync(int contactId)
    {
        var contact = await _context.Contacts.FindAsync(contactId);
        if (contact == null)
            return false;

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddToGroupAsync(int contactId, int groupId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Groups)
            .FirstOrDefaultAsync(c => c.Id == contactId);

        var group = await _context.ContactGroups.FindAsync(groupId);

        if (contact == null || group == null)
            return false;

        if (contact.Groups.Any(g => g.Id == groupId))
            return true;

        contact.Groups.Add(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromGroupAsync(int contactId, int groupId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Groups)
            .FirstOrDefaultAsync(c => c.Id == contactId);

        if (contact == null)
            return false;

        var group = contact.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null)
            return false;

        contact.Groups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ContactGroup>> GetGroupsAsync(int contactId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Groups)
            .FirstOrDefaultAsync(c => c.Id == contactId);

        return contact?.Groups.ToList() ?? new List<ContactGroup>();
    }

    public async Task<List<Contact>> ImportContactsAsync(int tenantId, Stream fileStream, FileType fileType)
    {
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
                contact.TenantId = tenantId;
                var importedContact = await CreateContactAsync(tenantId, contact);
                importedContacts.Add(importedContact);
            }
            catch
            {
                continue;
            }
        }

        return importedContacts;
    }

    public async Task<(byte[] FileContents, string ContentType, string FileName)> ExportContactsAsync(
        int tenantId, FileType fileType, int? groupId = null)
    {
        var contacts = await GetContactsAsync(tenantId, groupId);
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        return fileType switch
        {
            FileType.Csv => (
                await ExportToCsvAsync(contacts),
                "text/csv",
                $"contacts_{timestamp}.csv"
            ),
            FileType.Excel => (
                await ExportToExcelAsync(contacts),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"contacts_{timestamp}.xlsx"
            ),
            FileType.Json => (
                await ExportToJsonAsync(contacts),
                "application/json",
                $"contacts_{timestamp}.json"
            ),
            FileType.Xml => (
                await ExportToXmlAsync(contacts),
                "application/xml",
                $"contacts_{timestamp}.xml"
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

    public async Task<bool> IsPhoneNumberValidAsync(string phoneNumber)
    {
        try
        {
            var number = _phoneUtil.Parse(phoneNumber, "TR");
            return _phoneUtil.IsValidNumber(number) && 
                   _phoneUtil.GetNumberType(number) == PhoneNumberType.MOBILE;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> FormatPhoneNumberAsync(string phoneNumber)
    {
        var number = _phoneUtil.Parse(phoneNumber, "TR");
        return _phoneUtil.Format(number, PhoneNumberFormat.E164);
    }

    public async Task<bool> IsPhoneNumberUniqueAsync(int tenantId, string phoneNumber, int? excludeContactId = null)
    {
        var query = _context.Contacts
            .Where(c => c.TenantId == tenantId && c.PhoneNumber == phoneNumber);

        if (excludeContactId.HasValue)
        {
            query = query.Where(c => c.Id != excludeContactId);
        }

        return !await query.AnyAsync();
    }
} 