using EnterpriseEmployeeManagementSystem.Domain.Entities;

namespace EnterpriseEmployeeManagementSystem.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiration();
    DateTime GetRefreshTokenExpiration();
}
