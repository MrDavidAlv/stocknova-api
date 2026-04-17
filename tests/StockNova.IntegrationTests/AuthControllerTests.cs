using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StockNova.Application.DTOs.Auth;
using StockNova.Application.DTOs.Common;
using StockNova.IntegrationTests.Fixtures;

namespace StockNova.IntegrationTests;

public class AuthControllerTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public AuthControllerTests(IntegrationTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn201()
    {
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid():N}@test.com",
            Password = "Test123!",
            FullName = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrEmpty();
        body.Data.RefreshToken.Should().NotBeNullOrEmpty();
        body.Data.User.Email.Should().Be(request.Email);
        body.Data.User.Role.Should().Be("Viewer");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn400()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var request = new RegisterRequest { Email = email, Password = "Test123!", FullName = "Test" };

        await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_AfterRegister_ShouldReturn200()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest { Email = email, Password = "Test123!", FullName = "Test" });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = email, Password = "Test123!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = "nobody@test.com", Password = "wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        var email = $"refresh_{Guid.NewGuid():N}@test.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest { Email = email, Password = "Test123!", FullName = "Test" });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token",
            new RefreshTokenRequest { RefreshToken = registerBody!.Data!.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }
}
