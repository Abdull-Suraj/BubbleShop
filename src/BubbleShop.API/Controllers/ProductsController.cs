using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Products.Commands.ActivateProduct;
using BubbleShop.Application.Features.Products.Commands.CreateProduct;
using BubbleShop.Application.Features.Products.Commands.DeactivateProduct;
using BubbleShop.Application.Features.Products.Commands.DeleteProduct;
using BubbleShop.Application.Features.Products.Commands.UpdateProduct;
using BubbleShop.Application.Features.Products.Commands.UpdateStock;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using BubbleShop.Application.Features.Products.Queries.GetProductById;
using BubbleShop.Application.Features.Products.Queries.SearchProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
[Produces("application/json")]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="businessId">Filter by business ID</param>
    /// <param name="category">Filter by category</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? businessId = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100)
            return BadRequest(new { error = "Page size cannot exceed 100" });

        var result = await _mediator.Send(new GetAllProductsQuery(pageNumber, pageSize, businessId, isActive, minPrice, maxPrice), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Search products by keyword with filters
    /// </summary>
    /// <param name="keyword">Search keyword</param>
    /// <param name="category">Filter by category</param>
    /// <param name="minPrice">Minimum price</param>
    /// <param name="maxPrice">Maximum price</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="businessId">Filter by business</param>
    /// <param name="sortBy">Sort field (name, price, createdAt)</param>
    /// <param name="sortDesc">Sort descending</param>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? businessId = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] bool sortDesc = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new SearchProductsQuery(keyword, minPrice, maxPrice, pageNumber, pageSize, businessId, sortBy, sortDesc), cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="command">Product data</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { errors = result.Errors });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { productId = result.Value });
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Updated product data</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { ProductId = id }, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Update product stock quantity
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Stock update command</param>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { ProductId = id }, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Activate a product
    /// </summary>
    /// <param name="id">Product ID</param>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivateProductCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Deactivate a product
    /// </summary>
    /// <param name="id">Product ID</param>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateProductCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Bulk create products
    /// </summary>
    /// <param name="commands">List of products to create</param>
    [HttpPost("bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BulkCreate([FromBody] List<CreateProductCommand> commands, CancellationToken cancellationToken)
    {
        var results = new List<object>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var command in commands)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsSuccess)
            {
                successCount++;
                results.Add(new { productId = result.Value, success = true });
            }
            else
            {
                failureCount++;
                results.Add(new { error = result.Error, success = false });
            }
        }

        return Ok(new { total = commands.Count, successCount, failureCount, results });
    }

    /// <summary>
    /// Export products to CSV
    /// </summary>
    // Update the Export method to handle the new query
    /// <summary>
    /// Export products to CSV or JSON
    /// </summary>
    [HttpGet("export")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Export(
        [FromQuery] string format = "csv",
        [FromQuery] string? category = null,
        [FromQuery] Guid? businessId = null,
        CancellationToken cancellationToken = default)
    {
        // Get all products (you might want to add filters)
        var result = await _mediator.Send(new GetAllProductsQuery(
            PageSize: int.MaxValue,
            BusinessId: businessId

        ), cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var products = result.Value;

        return format.ToLower() switch
        {
            "csv" => ExportToCsv(products),
            "json" => ExportToJson(products),
            "excel" => ExportToExcel(products),
            _ => BadRequest(new { error = "Unsupported format. Supported formats: csv, json, excel" })
        };
    }

    private IActionResult ExportToCsv(IEnumerable<ProductDto> products)
    {
        var csv = ConvertProductsToCsv(products);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"products_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    private IActionResult ExportToJson(IEnumerable<ProductDto> products)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(products, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"products_{DateTime.Now:yyyyMMdd_HHmmss}.json");
    }

    private IActionResult ExportToExcel(IEnumerable<ProductDto> products)
    {
        // For Excel export, you'll need to install EPPlus or ClosedXML
        // This is a placeholder - you can implement with your preferred library
        return Ok(new { message = "Excel export coming soon", data = products });
    }

    private string ConvertProductsToCsv(IEnumerable<ProductDto> products)
    {
        var csv = new System.Text.StringBuilder();

        // Header
        csv.AppendLine("Id,Name,Description,Price,StockQuantity,Category,ImageUrl,IsActive,CreatedAt,LastModifiedAt");

        // Data rows
        foreach (var product in products)
        {
            csv.AppendLine($"{product.Id}," +
                          $"{EscapeCsv(product.Name)}," +
                          $"{EscapeCsv(product.Description)}," +
                          $"{product.Price}," +
                          $"{product.StockQuantity}," +
                          $"{EscapeCsv(product.ImageUrl)}," +
                          $"{product.IsActive}," +
                          $"{product.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"{(product.LastModifiedAt.HasValue ? product.LastModifiedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "")}");
        }

        return csv.ToString();
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // If the value contains comma, newline, or double quote, enclose it in quotes
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }

}