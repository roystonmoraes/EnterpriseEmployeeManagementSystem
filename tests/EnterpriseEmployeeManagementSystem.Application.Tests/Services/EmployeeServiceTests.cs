using EnterpriseEmployeeManagementSystem.Application.DTOs;
using EnterpriseEmployeeManagementSystem.Application.DTOs.Employees;
using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Application.Services;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseEmployeeManagementSystem.Application.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly Mock<ILogger<EmployeeService>> _loggerMock;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<EmployeeService>>();

        _service = new EmployeeService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateEmployee()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "9876543210",
            DateOfBirth = new DateOnly(1995, 1, 15),
            HireDate = new DateOnly(2024, 1, 10),
            DepartmentId = 1,
        };

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback<Employee, CancellationToken>(
                (employee, _) =>
                {
                    employee.Id = 1;
                }
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.True(result.IsActive);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            PhoneNumber = "9876543210",
            DateOfBirth = new DateOnly(1995, 1, 15),
            HireDate = new DateOnly(2024, 1, 10),
            DepartmentId = 1,
        };

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(request));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ShouldReturnEmployee()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "9876543210",
            DateOfBirth = new DateOnly(1995, 1, 15),
            HireDate = new DateOnly(2024, 1, 10),
            DepartmentId = 1,
            IsActive = true,
            Department = new Department { Id = 1, Name = "Engineering" },
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("Engineering", result.DepartmentName);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedEmployees()
    {
        // Arrange
        var request = new EmployeeQueryRequest { PageNumber = 1, PageSize = 10 };

        var employees = new List<Employee>
        {
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                DepartmentId = 1,
                IsActive = true,
            },
            new()
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                DepartmentId = 2,
                IsActive = true,
            },
        };

        _repositoryMock
            .Setup(r =>
                r.GetPagedAsync(
                    request.Search,
                    request.ActiveOnly,
                    request.PageNumber,
                    request.PageSize,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((employees, 2));

        // Act
        var result = await _service.GetAllAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.Items[0].Id);
        Assert.Equal(2, result.Items[1].Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageNumberIsLessThanOne_ShouldSetItToOne()
    {
        // Arrange
        var request = new EmployeeQueryRequest { PageNumber = 0, PageSize = 10 };

        _repositoryMock
            .Setup(r =>
                r.GetPagedAsync(
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<Employee>(), 0));

        // Act
        var result = await _service.GetAllAsync(request);

        // Assert
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageSizeIsGreaterThan100_ShouldSetItTo100()
    {
        // Arrange
        var request = new EmployeeQueryRequest { PageNumber = 1, PageSize = 500 };

        _repositoryMock
            .Setup(r =>
                r.GetPagedAsync(
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    1,
                    100,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<Employee>(), 0));

        // Act
        var result = await _service.GetAllAsync(request);

        // Assert
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new UpdateEmployeeRequest
        {
            FirstName = "Updated",
            LastName = "Employee",
            Email = "updated@example.com",
            PhoneNumber = "9876543210",
            DateOfBirth = new DateOnly(1995, 1, 15),
            HireDate = new DateOnly(2024, 1, 10),
            DepartmentId = 1,
            IsActive = true,
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(999, request));
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailBelongsToAnotherEmployee_ShouldThrowConflictException()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DepartmentId = 1,
            IsActive = true,
        };

        var request = new UpdateEmployeeRequest
        {
            FirstName = "John",
            LastName = "Updated",
            Email = "jane@example.com",
            PhoneNumber = "9876543210",
            DateOfBirth = new DateOnly(1995, 1, 15),
            HireDate = new DateOnly(2024, 1, 10),
            DepartmentId = 1,
            IsActive = true,
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync("jane@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(1, request));

        _repositoryMock.Verify(r => r.Update(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEmployee()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DepartmentId = 1,
            IsActive = true,
        };

        var request = new UpdateEmployeeRequest
        {
            FirstName = "Johnny",
            LastName = "Updated",
            Email = "john.updated@example.com",
            PhoneNumber = "9999999999",
            DateOfBirth = new DateOnly(1995, 1, 15),
            HireDate = new DateOnly(2024, 1, 10),
            DepartmentId = 2,
            IsActive = true,
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _repositoryMock
            .Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.Equal("Johnny", result.FirstName);
        Assert.Equal("Updated", result.LastName);
        Assert.Equal("john.updated@example.com", result.Email);
        Assert.Equal(2, result.DepartmentId);
        Assert.True(result.IsActive);

        _repositoryMock.Verify(r => r.Update(employee), Times.Once);

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenEmployeeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeactivateAsync(999));
    }

    [Fact]
    public async Task DeactivateAsync_WhenEmployeeIsActive_ShouldDeactivateEmployee()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DepartmentId = 1,
            IsActive = true,
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        await _service.DeactivateAsync(1);

        // Assert
        Assert.False(employee.IsActive);

        _repositoryMock.Verify(r => r.Update(employee), Times.Once);

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenEmployeeIsAlreadyInactive_ShouldDoNothing()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            DepartmentId = 1,
            IsActive = false,
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        await _service.DeactivateAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.Update(It.IsAny<Employee>()), Times.Never);

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
