using EnterpriseEmployeeManagementSystem.Application.DTOs.Auth;

namespace EnterpriseEmployeeManagementSystem.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    );

    Task<LoginResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default
    );
}
