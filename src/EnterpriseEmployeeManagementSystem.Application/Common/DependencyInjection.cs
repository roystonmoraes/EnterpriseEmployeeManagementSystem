using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseEmployeeManagementSystem.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();

        return services;
    }
}