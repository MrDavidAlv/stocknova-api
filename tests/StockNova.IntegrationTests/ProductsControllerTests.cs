using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using StockNova.Application.DTOs.Auth;
using StockNova.Application.DTOs.Categories;
using StockNova.Application.DTOs.Common;
using StockNova.Application.DTOs.Products;
using StockNova.IntegrationTests.Fixtures;

namespace StockNova.IntegrationTests;

public class ProductsControllerTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(IntegrationTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        // The seeder creates admin@stocknova.com / Admin123!
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest { Email = "admin@stocknova.com", Password = "Admin123!" });
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return body!.Data!.AccessToken;
    }

    private void SetAuth(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/v1/products?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductResponse>>>();
        body!.Success.Should().BeTrue();
        body.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithAuth_ShouldReturn201()
    {
        var token = await GetAdminTokenAsync();
        SetAuth(token);

        var request = new CreateProductRequest
        {
            ProductName = $"Test Product {Guid.NewGuid():N}",
            UnitPrice = 99.99m,
            UnitsInStock = 10,
            Discontinued = false
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();
        body!.Data!.ProductName.Should().Be(request.ProductName);
        body.Data.ProductId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_WithoutAuth_ShouldReturn401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest { ProductName = "Unauthorized" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturn200()
    {
        var token = await GetAdminTokenAsync();
        SetAuth(token);

        // Create a product first
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest { ProductName = "GetById Test", UnitPrice = 50m, Discontinued = false });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/v1/products/{created!.Data!.ProductId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDetailResponse>>();
        body!.Data!.ProductName.Should().Be("GetById Test");
    }

    [Fact]
    public async Task GetById_WhenNotExists_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/v1/products/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithAuth_ShouldReturn200()
    {
        var token = await GetAdminTokenAsync();
        SetAuth(token);

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest { ProductName = "Before Update", UnitPrice = 10m, Discontinued = false });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();

        // Update
        var updateRequest = new UpdateProductRequest
        {
            ProductName = "After Update",
            UnitPrice = 20m,
            Discontinued = false
        };
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{created!.Data!.ProductId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();
        body!.Data!.ProductName.Should().Be("After Update");
        body.Data.UnitPrice.Should().Be(20m);
    }

    [Fact]
    public async Task Delete_WithAdminAuth_ShouldReturn200()
    {
        var token = await GetAdminTokenAsync();
        SetAuth(token);

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products",
            new CreateProductRequest { ProductName = "To Delete", UnitPrice = 5m, Discontinued = false });
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();

        // Delete
        var response = await _client.DeleteAsync($"/api/v1/products/{created!.Data!.ProductId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify it's gone (soft delete + global filter)
        _client.DefaultRequestHeaders.Authorization = null;
        var getResponse = await _client.GetAsync($"/api/v1/products/{created.Data.ProductId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithViewerAuth_ShouldReturn403()
    {
        // Register a viewer
        var email = $"viewer_{Guid.NewGuid():N}@test.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest { Email = email, Password = "Test123!", FullName = "Viewer" });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        SetAuth(registerBody!.Data!.AccessToken);

        var response = await _client.DeleteAsync("/api/v1/products/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
