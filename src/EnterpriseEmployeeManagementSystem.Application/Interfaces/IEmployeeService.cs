using EnterpriseEmployeeManagementSystem.Application.DTOs.Employees;

namespace EnterpriseEmployeeManagementSystem.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeDto> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(
        CancellationToken cancellationToken = default);
}