using EnterpriseEmployeeManagementSystem.Application.DTOs.Departments;
using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Application.Services;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseEmployeeManagementSystem.Application.Tests.Services;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _repositoryMock;
    private readonly Mock<ILogger<DepartmentService>> _loggerMock;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _repositoryMock = new Mock<IDepartmentRepository>();
        _loggerMock = new Mock<ILogger<DepartmentService>>();

        _service = new DepartmentService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnDepartments()
    {
        // Arrange
        var departments = new List<Department>
        {
            new() { Id = 1, Name = "Engineering" },
            new() { Id = 2, Name = "Finance" },
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(departments);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Engineering", result[0].Name);
        Assert.Equal("Finance", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDepartmentDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(999));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDepartment()
    {
        // Arrange
        var request = new CreateDepartmentRequest { Name = "Marketing" };

        _repositoryMock
            .Setup(r => r.GetByNameAsync("Marketing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()))
            .Callback<Department, CancellationToken>(
                (department, _) =>
                {
                    department.Id = 10;
                }
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.Equal(10, result.Id);
        Assert.Equal("Marketing", result.Name);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenDepartmentAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var request = new CreateDepartmentRequest { Name = "Engineering" };

        _repositoryMock
            .Setup(r => r.GetByNameAsync("Engineering", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = 1, Name = "Engineering" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentHasEmployees_ShouldThrowConflictException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = 1, Name = "Engineering" });

        _repositoryMock
            .Setup(r => r.HasEmployeesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(1));

        _repositoryMock.Verify(r => r.Remove(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentHasNoEmployees_ShouldDeleteDepartment()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "Engineering" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _repositoryMock
            .Setup(r => r.HasEmployeesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.Remove(department), Times.Once);

        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
