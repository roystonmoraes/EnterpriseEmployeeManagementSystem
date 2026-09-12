using EnterpriseEmployeeManagementSystem.Application.DTOs.Departments;
using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EnterpriseEmployeeManagementSystem.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        ILogger<DepartmentService> logger
    )
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        return departments.Select(MapToResponse).ToList();
    }

    public async Task<DepartmentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
            throw new NotFoundException($"Department with ID {id} was not found.");

        return MapToResponse(department);
    }

    public async Task<DepartmentResponse> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var name = request.Name.Trim();

        var existingDepartment = await _departmentRepository.GetByNameAsync(
            name,
            cancellationToken
        );

        if (existingDepartment is not null)
            throw new ConflictException($"Department '{name}' already exists.");

        var department = new Department { Name = name };

        await _departmentRepository.AddAsync(department, cancellationToken);

        await _departmentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Department {DepartmentId} created with name {DepartmentName}.",
            department.Id,
            department.Name
        );

        return MapToResponse(department);
    }

    public async Task<DepartmentResponse> UpdateAsync(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
            throw new NotFoundException($"Department with ID {id} was not found.");

        var name = request.Name.Trim();

        var existingDepartment = await _departmentRepository.GetByNameAsync(
            name,
            cancellationToken
        );

        if (existingDepartment is not null && existingDepartment.Id != id)
        {
            throw new ConflictException($"Department '{name}' already exists.");
        }

        department.Name = name;

        await _departmentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department {DepartmentId} updated.", department.Id);

        return MapToResponse(department);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id, cancellationToken);

        if (department is null)
            throw new NotFoundException($"Department with ID {id} was not found.");

        var hasEmployees = await _departmentRepository.HasEmployeesAsync(id, cancellationToken);

        if (hasEmployees)
        {
            throw new ConflictException(
                "Cannot delete a department that has employees assigned to it."
            );
        }

        _departmentRepository.Remove(department);

        await _departmentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department {DepartmentId} deleted.", id);
    }

    private static DepartmentResponse MapToResponse(Department department)
    {
        return new DepartmentResponse { Id = department.Id, Name = department.Name };
    }
}
