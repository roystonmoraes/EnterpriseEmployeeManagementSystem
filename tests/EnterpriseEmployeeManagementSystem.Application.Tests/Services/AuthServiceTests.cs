using EnterpriseEmployeeManagementSystem.Application.DTOs.Auth;
using EnterpriseEmployeeManagementSystem.Application.Exceptions;
using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Application.Services;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseEmployeeManagementSystem.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _service = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = "hashed-password",
            Role = "Admin",
            IsActive = true,
        };

        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(15);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("Admin@123", "hashed-password"))
            .Returns(true);

        _jwtTokenServiceMock.Setup(j => j.GenerateAccessToken(user)).Returns(accessToken);

        _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns(refreshToken);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpiration())
            .Returns(accessTokenExpiration);

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpiration())
            .Returns(refreshTokenExpiration);

        // Act
        var result = await _service.LoginAsync(
            new LoginRequest { Username = "admin", Password = "Admin@123" }
        );

        // Assert
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
        Assert.Equal(accessTokenExpiration, result.AccessTokenExpiresAt);

        Assert.Equal(refreshToken, user.RefreshToken);
        Assert.Equal(refreshTokenExpiration, user.RefreshTokenExpiresAt);
        Assert.NotNull(user.LastLoginAt);

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUsername_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Username = "unknown", Password = "password" })
        );

        _passwordHasherMock.Verify(
            p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            PasswordHash = "hashed-password",
            Role = "Admin",
            IsActive = true,
        };

        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("wrong-password", "hashed-password"))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(
                new LoginRequest { Username = "admin", Password = "wrong-password" }
            )
        );

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "inactive",
            PasswordHash = "hashed-password",
            Role = "Employee",
            IsActive = false,
        };

        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync("inactive", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword("password", "hashed-password"))
            .Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest { Username = "inactive", Password = "password" })
        );

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var oldRefreshToken = "old-refresh-token";
        var newRefreshToken = "new-refresh-token";

        var user = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = "hashed-password",
            Role = "Admin",
            IsActive = true,
            RefreshToken = oldRefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(5),
        };

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(15);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(oldRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock.Setup(j => j.GenerateAccessToken(user)).Returns("new-access-token");

        _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns(newRefreshToken);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpiration())
            .Returns(accessTokenExpiration);

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpiration())
            .Returns(refreshTokenExpiration);

        // Act
        var result = await _service.RefreshTokenAsync(
            new RefreshTokenRequest { RefreshToken = oldRefreshToken }
        );

        // Assert
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal(newRefreshToken, result.RefreshToken);
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);

        Assert.Equal(newRefreshToken, user.RefreshToken);
        Assert.Equal(refreshTokenExpiration, user.RefreshTokenExpiresAt);

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "invalid-token" })
        );

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Role = "Admin",
            IsActive = true,
            RefreshToken = "expired-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(-1),
        };

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "expired-token" })
        );

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "inactive",
            Role = "Employee",
            IsActive = false,
            RefreshToken = "valid-token",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(5),
        };

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "valid-token" })
        );

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
