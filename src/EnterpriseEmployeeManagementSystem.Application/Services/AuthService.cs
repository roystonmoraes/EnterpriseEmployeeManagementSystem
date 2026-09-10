using EnterpriseEmployeeManagementSystem.Application.DTOs.Auth;
using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnterpriseEmployeeManagementSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger
    )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username {Username}.", request.Username);

            throw new UnauthorizedException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user {Username}.", user.Username);

            throw new UnauthorizedException("User account is inactive.");
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var accessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration();

        var refreshTokenExpiresAt = _jwtTokenService.GetRefreshTokenExpiration();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        user.LastLoginAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            Username = user.Username,
            Role = user.Role,
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userRepository.GetByRefreshTokenAsync(
            request.RefreshToken,
            cancellationToken
        );

        if (user is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("User account is inactive");
        }

        if (user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token has expired");
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var accessTokenExpiresAt = _jwtTokenService.GetAccessTokenExpiration();
        var refreshTokenExpiresAt = _jwtTokenService.GetRefreshTokenExpiration();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            Username = user.Username,
            Role = user.Role,
        };
    }
}
