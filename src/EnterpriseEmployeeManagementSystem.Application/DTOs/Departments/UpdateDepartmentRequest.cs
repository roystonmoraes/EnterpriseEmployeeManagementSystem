using System.ComponentModel.DataAnnotations;

namespace EnterpriseEmployeeManagementSystem.Application.DTOs.Departments;

public class UpdateDepartmentRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
