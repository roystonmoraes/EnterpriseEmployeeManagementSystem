namespace EnterpriseEmployeeManagementSystem.Application.DTOs.Employees;

public class EmployeeQueryRequest
{
    public string? Search { get; set; }

    public bool? ActiveOnly { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}