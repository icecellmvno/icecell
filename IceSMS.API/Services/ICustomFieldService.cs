using IceSMS.API.Models.Domain;

namespace IceSMS.API.Services;

public interface ICustomFieldService
{
    Task<List<CustomField>> GetFieldsAsync(int tenantId);
    Task<CustomField?> GetFieldByIdAsync(int fieldId);
    Task<CustomField> CreateFieldAsync(int tenantId, CustomField field);
    Task<CustomField> UpdateFieldAsync(int fieldId, CustomField field);
    Task<bool> DeleteFieldAsync(int fieldId);
    Task<bool> ValidateValueAsync(CustomField field, string value);
    Task<List<string>> GetOptionsAsync(int fieldId);
} 