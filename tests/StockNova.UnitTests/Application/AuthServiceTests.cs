using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockNova.Application.Configuration;
using StockNova.Application.DTOs.Auth;
using StockNova.Application.Interfaces;
using StockNova.Application.Services;
using StockNova.Domain.Entities;
using StockNova.Domain.Enums;
using StockNova.Domain.Interfaces.Repositories;
using StockNova.Domain.Interfaces.Services;

namespace StockNova.UnitTests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenGenerator> _jwtMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtTokenGenerator>();
        _hasherMock = new Mock<IPasswordHasher>();
        var logger = new Mock<ILogger<AuthService>>();

        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "TestKey-Must-Be-At-Least-32-Characters-Long!!",
            Issuer = "Test",
            Audience = "Test",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        });

        _jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("test-access-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("test-refresh-token");

        _service = new AuthService(
            _userRepoMock.Object,
            _jwtMock.Object,
            _hasherMock.Object,
            jwtSettings,
            logger.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
    {
        var user = new User
        {
            UserId = 1,
            Email = "admin@test.com",
            PasswordHash = "hashed",
            FullName = "Admin",
            Role = UserRole.Admin,
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Password1!", "hashed")).Returns(true);

        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "admin@test.com",
            Password = "Password1!"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("test-access-token");
        result.Value.RefreshToken.Should().Be("test-refresh-token");
        result.Value.User.Email.Should().Be("admin@test.com");
        result.Value.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("bad@test.com", default)).ReturnsAsync((User?)null);

        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "bad@test.com",
            Password = "Password1!"
        });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnFailure()
    {
        var user = new User { Email = "inactive@test.com", IsActive = false };
        _userRepoMock.Setup(r => r.GetByEmailAsync("inactive@test.com", default)).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "inactive@test.com",
            Password = "Password1!"
        });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnFailure()
    {
        var user = new User
        {
            Email = "admin@test.com",
            PasswordHash = "hashed",
            IsActive = true
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "admin@test.com",
            Password = "wrong"
        });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ShouldReturnSuccess()
    {
        _userRepoMock.Setup(r => r.ExistsByEmailAsync("new@test.com", default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("Password1!")).Returns("hashed-password");

        var result = await _service.RegisterAsync(new RegisterRequest
        {
            Email = "new@test.com",
            Password = "Password1!",
            FullName = "New User"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().NotBeNullOrEmpty();
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "new@test.com" &&
            u.FullName == "New User" &&
            u.Role == UserRole.Viewer), default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        _userRepoMock.Setup(r => r.ExistsByEmailAsync("exists@test.com", default)).ReturnsAsync(true);

        var result = await _service.RegisterAsync(new RegisterRequest
        {
            Email = "exists@test.com",
            Password = "Password1!",
            FullName = "Existing"
        });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already registered");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        var user = new User
        {
            UserId = 1,
            Email = "admin@test.com",
            FullName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            RefreshToken = "valid-refresh",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(1)
        };

        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("valid-refresh", default)).ReturnsAsync(user);

        var result = await _service.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnFailure()
    {
        var user = new User
        {
            IsActive = true,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1) // expired
        };

        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("expired", default)).ReturnsAsync(user);

        var result = await _service.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "expired"
        });

        result.IsSuccess.Should().BeFalse();
    }
}
