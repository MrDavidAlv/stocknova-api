using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockNova.API.Extensions;
using StockNova.Application.DTOs.Common;
using StockNova.Application.DTOs.Products;
using StockNova.Application.Interfaces;

namespace StockNova.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IAuditService _auditService;

    public ProductsController(IProductService productService, IAuditService auditService)
    {
        _productService = productService;
        _auditService = auditService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilterParams filterParams)
    {
        var result = await _productService.GetAllAsync(filterParams);
        return Ok(ApiResponse<PagedResult<ProductResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ProductDetailResponse>.Fail(result.Error!));

        return Ok(ApiResponse<ProductDetailResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var userId = User.GetUserId();
        var result = await _productService.CreateAsync(request, userId);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ProductResponse>.Fail(result.Error!));

        await _auditService.LogAsync(userId, User.GetEmail(), "Product.Create",
            "Product", result.Value!.ProductId.ToString(), newValues: System.Text.Json.JsonSerializer.Serialize(request));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ProductId },
            ApiResponse<ProductResponse>.Ok(result.Value!, "Product created"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        var userId = User.GetUserId();
        var result = await _productService.UpdateAsync(id, request, userId);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<ProductResponse>.Fail(result.Error!));

        await _auditService.LogAsync(userId, User.GetEmail(), "Product.Update",
            "Product", id.ToString(), newValues: System.Text.Json.JsonSerializer.Serialize(request));

        return Ok(ApiResponse<ProductResponse>.Ok(result.Value!, "Product updated"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var result = await _productService.DeleteAsync(id);
        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error!));

        await _auditService.LogAsync(userId, User.GetEmail(), "Product.Delete", "Product", id.ToString());

        return Ok(ApiResponse<object>.Ok(null!, "Product deleted"));
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateRequest request)
    {
        var result = await _productService.BulkCreateAsync(request);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<int>.Fail(result.Error!));

        await _auditService.LogAsync(User.GetUserId(), User.GetEmail(), "Product.BulkCreate",
            "Product", message: $"Bulk insert of {result.Value} products");

        return Ok(ApiResponse<int>.Ok(result.Value, $"{result.Value} products created"));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ImportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ImportResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<ImportResult>.Fail("No file provided"));

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<ImportResult>.Fail("Only CSV files are accepted"));

        using var stream = file.OpenReadStream();
        var result = await _productService.ImportFromCsvAsync(stream);
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<ImportResult>.Fail(result.Error!));

        await _auditService.LogAsync(User.GetUserId(), User.GetEmail(), "Product.Import",
            "Product", message: $"CSV import: {result.Value!.Imported} imported, {result.Value.Failed} failed");

        return Ok(ApiResponse<ImportResult>.Ok(result.Value!, $"{result.Value.Imported} products imported"));
    }
}
