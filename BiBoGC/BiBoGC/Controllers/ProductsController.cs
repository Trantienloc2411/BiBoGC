using InventoryManagement.Application.Commands.AddBatch;
using InventoryManagement.Application.Commands.AddProductVariant;
using InventoryManagement.Application.Commands.CreateProduct;
using InventoryManagement.Application.Commands.DeleteBatch;
using InventoryManagement.Application.Commands.DeleteProduct;
using InventoryManagement.Application.Commands.DeleteProductVariant;
using InventoryManagement.Application.Commands.UpdateBatch;
using InventoryManagement.Application.Commands.UpdateProduct;
using InventoryManagement.Application.Commands.UpdateProductVariant;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Queries.GetBatch;
using InventoryManagement.Application.Queries.GetBatches;
using InventoryManagement.Application.Queries.GetProduct;
using InventoryManagement.Application.Queries.GetProducts;
using InventoryManagement.Application.Queries.GetProductVariantsByProductId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiBoGC.Controllers;

/// <summary>
/// API Controller for Product management operations
/// Handles CRUD operations for products in the inventory system
/// </summary>
/// <remarks>
/// Base path: /api/products
/// 
/// This controller provides endpoints for:
/// - Creating new products
/// - Retrieving product information (single or list with pagination)
/// - Updating product details
/// - Deleting products (soft delete)
/// - Adding batches to products with expiry tracking
/// 
/// All responses follow a consistent format with success/error indicators.
/// Validation errors return 400 Bad Request with detailed error messages.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of products
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of products with optional search functionality.
    /// 
    /// **Search behavior:**
    /// - Searches across product name, SKU, and description
    /// - Case-insensitive matching
    /// - Results sorted alphabetically by name
    /// 
    /// **Pagination:**
    /// - Page numbers are 1-based
    /// - Default page size is 10, maximum is 100
    /// - Response includes total count and page metadata
    /// 
    /// **Response fields:**
    /// - items: Array of product objects
    /// - pageNumber: Current page (1-based)
    /// - pageSize: Items per page
    /// - totalCount: Total items across all pages
    /// - totalPages: Total number of pages
    /// - hasPrevious: Boolean indicating if previous page exists
    /// - hasNext: Boolean indicating if next page exists
    /// 
    /// **Example usage for frontend/AI:**
    /// ```
    /// GET /api/products?pageNumber=1&amp;pageSize=10&amp;searchTerm=coca
    /// ```
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Items per page (1-100, default: 10)</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns paginated product list</response>
    /// <response code="400">Invalid pagination parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Seller,Administrator")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a single product by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific product including recent batches.
    /// 
    /// **Response includes:**
    /// - Basic product info: id, name, sku, price, description, status
    /// - Stock information: totalStock, availableStock, expiredStock, expiringSoonStock
    /// - Batch tracking: requiresBatchTracking flag
    /// - Timestamps: createdAt, updatedAt
    /// - Recent batches: up to 5 most recent batches
    /// 
    /// **Stock calculations:**
    /// - totalStock: Sum of all non-expired batch quantities
    /// - availableStock: Same as totalStock (batches with quantity > 0)
    /// - expiredStock: Sum of quantities in expired batches
    /// - expiringSoonStock: Sum of quantities expiring within 30 days
    /// 
    /// **Example usage for frontend/AI:**
    /// ```
    /// GET /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// ```
    /// </remarks>
    /// <param name="id">Product ID (GUID)</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns product details</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var query = new GetProductQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy sản phẩm",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <remarks>
    /// Creates a new product in the inventory system.
    /// 
    /// **Required fields:**
    /// - name: Product name (max 200 characters)
    /// - sku: Unique Stock Keeping Unit (max 20 characters, alphanumeric with - and _)
    /// - price: Product price (must be >= 0)
    /// 
    /// **Optional fields:**
    /// - description: Product description (max 1000 characters)
    /// - requiresBatchTracking: Enable batch/expiry tracking (default: false)
    /// 
    /// **SKU rules:**
    /// - Must be unique across all products
    /// - Automatically converted to uppercase
    /// - Only allows: letters, numbers, hyphens, underscores
    /// 
    /// **Example request body:**
    /// ```json
    /// {
    ///   "name": "Coca Cola 330ml",
    ///   "sku": "SKU-COCA-330",
    ///   "price": 15000,
    ///   "description": "Nước ngọt có gas Coca Cola lon 330ml",
    ///   "requiresBatchTracking": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Product creation data</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Validation error or SKU already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi tạo sản phẩm",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <remarks>
    /// Updates product information. Only provided fields will be updated.
    /// 
    /// **Updatable fields:**
    /// - name: New product name
    /// - price: New product price (currently the only field that updates via domain logic)
    /// - description: New product description
    /// 
    /// **Note:** SKU cannot be changed after creation.
    /// 
    /// **Example request body:**
    /// ```json
    /// {
    ///   "price": 18000
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Product ID to update</param>
    /// <param name="command">Update data</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Product not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        // Ensure ID matches
        if (id != command.Id && command.Id != Guid.Empty)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "ID không khớp",
                Detail = "ID trong URL và body phải giống nhau.",
                Status = StatusCodes.Status400BadRequest
            });

        var updateCommand = command with { Id = id };
        var result = await _mediator.Send(updateCommand);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("Không tìm thấy"))
                return NotFound(new ProblemDetails
                {
                    Title = "Không tìm thấy sản phẩm",
                    Detail = errorMessage,
                    Status = StatusCodes.Status404NotFound
                });

            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi cập nhật sản phẩm",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <remarks>
    /// Performs a soft delete on the product.
    /// The product will be marked as deleted but data is preserved in the database.
    /// Deleted products will not appear in search results.
    /// 
    /// **Example usage for frontend/AI:**
    /// ```
    /// DELETE /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// ```
    /// </remarks>
    /// <param name="id">Product ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Product deleted successfully</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy sản phẩm",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        return NoContent();
    }

    /// <summary>
    /// Add a batch to a product
    /// </summary>
    /// <remarks>
    /// Adds a new batch with quantity to an existing product.
    /// Used for inventory receiving/stock-in operations.
    /// 
    /// **Required fields:**
    /// - batchNumber: Unique batch code within the product (max 50 chars)
    /// - quantity: Number of items (must be > 0)
    /// - manufacturingDate: Date of manufacture (cannot be in future)
    /// - expirationDate: Expiry date (must be after manufacturing date)
    /// 
    /// **Optional fields:**
    /// - costPrice: Purchase/cost price per unit (default: 0)
    /// 
    /// **Batch number rules:**
    /// - Must be unique within the product
    /// - Automatically converted to uppercase
    /// - Only allows: letters, numbers, hyphens, underscores
    /// 
    /// **Example request body:**
    /// ```json
    /// {
    ///   "batchNumber": "BATCH-2024-001",
    ///   "quantity": 100,
    ///   "manufacturingDate": "2024-01-15",
    ///   "expirationDate": "2025-01-15",
    ///   "costPrice": 12000
    /// }
    /// ```
    /// 
    /// **Response includes:**
    /// - Batch details with calculated fields
    /// - isExpired: Whether batch is currently expired
    /// - daysUntilExpiration: Days remaining until expiry
    /// - isExpiringSoon: Whether batch expires within 30 days
    /// </remarks>
    /// <param name="productId">Product ID to add batch to</param>
    /// <param name="command">Batch data</param>
    /// <returns>Created batch</returns>
    /// <response code="201">Batch added successfully</response>
    /// <response code="400">Validation error or batch number already exists</response>
    /// <response code="404">Product not found</response>
    [HttpPost("{productId:guid}/batches")]
    [ProducesResponseType(typeof(ProductBatchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddBatch(Guid productId, [FromBody] AddBatchCommand command)
    {
        // Ensure ProductId matches
        if (productId != command.ProductId && command.ProductId != Guid.Empty)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "ID không khớp",
                Detail = "ID sản phẩm trong URL và body phải giống nhau.",
                Status = StatusCodes.Status400BadRequest
            });

        var addCommand = command with { ProductId = productId };
        var result = await _mediator.Send(addCommand);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("Không tìm thấy"))
                return NotFound(new ProblemDetails
                {
                    Title = "Không tìm thấy sản phẩm",
                    Detail = errorMessage,
                    Status = StatusCodes.Status404NotFound
                });

            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi thêm lô hàng",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return CreatedAtAction(nameof(GetBatch), new { productId = productId, batchId = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Get all batches for a product
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of batches for a specific product.
    /// 
    /// **Filtering:**
    /// - includeExpired: Include expired batches (default: false)
    /// 
    /// **Sorting:**
    /// - sortBy: ExpirationDate, ManufacturingDate, Quantity, BatchNumber, CreatedAt
    /// - Default: ExpirationDate (FEFO - First Expiry, First Out)
    /// </remarks>
    /// <param name="productId">Product ID</param>
    /// <param name="pageNumber">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Items per page (1-100, default: 10)</param>
    /// <param name="includeExpired">Include expired batches (default: false)</param>
    /// <param name="sortBy">Sort field (default: ExpirationDate)</param>
    /// <param name="sortDescending">Sort descending (default: false)</param>
    /// <returns>Paginated list of batches</returns>
    [HttpGet("{productId:guid}/batches")]
    [ProducesResponseType(typeof(PaginatedResult<ProductBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBatches(
        Guid productId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeExpired = false,
        [FromQuery] string? sortBy = "ExpirationDate",
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetBatchesQuery
        {
            ProductId = productId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeExpired = includeExpired,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific batch by ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="batchId">Batch ID</param>
    /// <returns>Batch details</returns>
    [HttpGet("{productId:guid}/batches/{batchId:guid}")]
    [ProducesResponseType(typeof(ProductBatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatch(Guid productId, Guid batchId)
    {
        var query = new GetBatchQuery { Id = batchId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy lô hàng",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        // Verify batch belongs to product
        if (result.Value!.ProductId != productId)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy lô hàng",
                Detail = "Lô hàng không thuộc sản phẩm này.",
                Status = StatusCodes.Status404NotFound
            });

        return Ok(result.Value);
    }

    /// <summary>
    /// Update a batch
    /// </summary>
    /// <remarks>
    /// Updates batch information. Only quantity can be updated.
    /// 
    /// **Updatable fields:**
    /// - quantity: New quantity (>= 0)
    /// 
    /// **Example request body:**
    /// ```json
    /// {
    ///   "quantity": 50
    /// }
    /// ```
    /// </remarks>
    /// <param name="productId">Product ID</param>
    /// <param name="batchId">Batch ID</param>
    /// <param name="request">Update data</param>
    /// <returns>Updated batch</returns>
    [HttpPut("{productId:guid}/batches/{batchId:guid}")]
    [ProducesResponseType(typeof(ProductBatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBatch(
        Guid productId,
        Guid batchId,
        [FromBody] UpdateBatchRequest request)
    {
        var command = new UpdateBatchCommand
        {
            Id = batchId,
            ProductId = productId,
            Quantity = request.Quantity,
            ManufacturingDate = request.ManufacturingDate,
            ExpirationDate = request.ExpirationDate,
            CostPrice = request.CostPrice
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ProblemDetails
                {
                    Title = "Không tìm thấy dữ liệu",
                    Detail = errorMessage,
                    Status = StatusCodes.Status404NotFound
                });

            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi cập nhật lô hàng",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a batch (soft delete)
    /// </summary>
    /// <remarks>
    /// Performs a soft delete on the batch.
    /// The batch will be marked as deleted but data is preserved.
    /// 
    /// **Warning:** Deleting a batch with remaining quantity will remove that stock.
    /// </remarks>
    /// <param name="productId">Product ID</param>
    /// <param name="batchId">Batch ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{productId:guid}/batches/{batchId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBatch(Guid productId, Guid batchId)
    {
        var command = new DeleteBatchCommand
        {
            Id = batchId,
            ProductId = productId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy lô hàng",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        return NoContent();
    }


    [HttpGet("{guid:guid}/variants", Name = "GetProductVariantByProductId")]
    [ProducesResponseType(typeof(IEnumerable<ProductVariantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductVariantByProductId(Guid guid)
    {
        var query = new GetProductVariantsByProductIdQuery { ProductId = guid };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = result.Errors.FirstOrDefault() ?? string.Empty,
                Detail = result.Errors.FirstOrDefault() ?? string.Empty,
                Status = StatusCodes.Status404NotFound
            });
        return Ok(result.Value);
    }

    [HttpPost("{productId:guid}/variants", Name = "CreateProductVariant")]
    [ProducesResponseType(typeof(ProductVariantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateProductVariant(Guid productId,
        [FromBody] CreateProductVariantCommand command)
    {
        var createCommand = new CreateProductVariantCommand
        {
            ProductId = productId,
            VariantName = command.VariantName,
            Barcode = command.Barcode,
            DisplayOrder = command.DisplayOrder,
            Unit = command.Unit,
            SalePrice = command.SalePrice,
            CostPrice = command.CostPrice,
            QuantityBaseUnit = command.QuantityBaseUnit
        };
        var result = await _mediator.Send(createCommand);

        if (!result.IsSuccess)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Đã xảy ra lỗi khi tạo biến thể mới.",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });
        return CreatedAtAction(nameof(GetProductVariantByProductId), new { guid = result.Value!.ProductId },
            result.Value);
    }


    [HttpPut("{productId:guid}/variants/{variantId:guid}", Name = "UpdateProductVariant")]
    [ProducesResponseType(typeof(ProductVariantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductVariant(Guid productId, Guid variantId,
        [FromBody] UpdateProductVariantCommand command)
    {
        if (productId != command.ProductId && command.ProductId != Guid.Empty ||
            variantId != command.ProductVariantId && command.ProductVariantId != Guid.Empty)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Id không khớp giữa URL query với body",
                Detail =
                    "Hãy chắc chắn rằng Id trên URL Query và Id trong body phải khớp với nhau và không được bỏ trống 1 trong 2!",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var updatedCommand = command with { ProductVariantId = variantId, ProductId = productId };
        var result = await _mediator.Send(updatedCommand);

        if (result.IsSuccess) return Ok(result.Value);
        if (result.Errors.Any(c => c.Contains("không tồn tại")))
            return NotFound(new ValidationProblemDetails
            {
                Title = "Đã xảy ra sai sót khi cập nhật.",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        return BadRequest(new ValidationProblemDetails
        {
            Title = "Đã xảy ra lỗi từ hệ thống! Hãy thử lại chốc lát!",
            Detail = result.Errors.FirstOrDefault(),
            Status = StatusCodes.Status400BadRequest
        });
    }

    [HttpDelete("{productId:guid}/variants/{variantId:guid}", Name = "DeleteProductVariant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductVariant(Guid productId, Guid variantId)
    {
        var command = new DeleteProductVariantCommand { ProductId = productId, Id = variantId };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("không tồn tại", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ProblemDetails
                {
                    Title = "Không tìm thấy biến thể sản phẩm",
                    Detail = errorMessage,
                    Status = StatusCodes.Status404NotFound
                });

            return BadRequest(new ProblemDetails
            {
                Title = "Hành động không thể thực thi!",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }
        return NoContent();
    }
}

#region Request DTOs for ProductsController

/// <summary>
/// Request model for updating a batch
/// </summary>
public record UpdateBatchRequest
{
    public int? Quantity { get; init; }
    public DateTime? ManufacturingDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public decimal? CostPrice { get; init; }
}

#endregion