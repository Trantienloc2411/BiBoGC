using InventoryManagement.Application.Commands.CreateStockTransaction;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Queries.GetStockTransaction;
using InventoryManagement.Application.Queries.GetStockTransactions;
using InventoryManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BiBoGC.Controllers;

/// <summary>
/// API Controller for Stock Transaction management operations
/// Handles stock movements including purchases, sales, adjustments, returns, etc.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class StockTransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockTransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of stock transactions
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of stock transactions with optional filters.
    /// 
    /// **Search behavior:**
    /// - Searches across product name, batch number, and supplier name
    /// - Case-insensitive matching
    /// 
    /// **Filtering:**
    /// - productId: Filter by specific product
    /// - supplierId: Filter by specific supplier
    /// - transactionType: Filter by transaction type (1=Purchase, 2=Sale, etc.)
    /// - fromDate/toDate: Filter by date range
    /// 
    /// **Transaction Types:**
    /// - 1: Purchase (Nhập hàng từ NCC)
    /// - 2: Sale (Bán hàng)
    /// - 3: AdjustmentIn (Điều chỉnh tăng)
    /// - 4: AdjustmentOut (Điều chỉnh giảm)
    /// - 5: Damage (Hư hỏng)
    /// - 6: Expiry (Hết hạn)
    /// - 7: Return (Khách trả hàng)
    /// - 8: SupplierReturn (Trả hàng NCC)
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Items per page (1-100, default: 10)</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="productId">Filter by product ID</param>
    /// <param name="supplierId">Filter by supplier ID</param>
    /// <param name="transactionType">Filter by transaction type</param>
    /// <param name="fromDate">Filter from date</param>
    /// <param name="toDate">Filter to date</param>
    /// <param name="sortBy">Sort field (default: TransactionDate)</param>
    /// <param name="sortDescending">Sort descending (default: true - newest first)</param>
    /// <returns>Paginated list of stock transactions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<StockTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStockTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] StockTransactionType? transactionType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = "TransactionDate",
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetStockTransactionsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            ProductId = productId,
            SupplierId = supplierId,
            TransactionType = transactionType,
            FromDate = fromDate,
            ToDate = toDate,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a single stock transaction by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific stock transaction.
    /// 
    /// **Response includes:**
    /// - Transaction details: id, transactionType, quantity, unitPrice, totalAmount
    /// - Product info: productId, productName, sku
    /// - Batch info: productBatchId, batchNumber (if applicable)
    /// - Supplier info: supplierId, supplierName (if applicable)
    /// - Timestamps: transactionDate
    /// - Notes: additional notes
    /// </remarks>
    /// <param name="id">Transaction ID (GUID)</param>
    /// <returns>Stock transaction details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStockTransaction(Guid id)
    {
        var query = new GetStockTransactionQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy giao dịch",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new stock transaction
    /// </summary>
    /// <remarks>
    /// Creates a new stock transaction and updates inventory accordingly.
    /// 
    /// **Required fields:**
    /// - productId: Product ID (GUID)
    /// - transactionType: Type of transaction (see enum values)
    /// - quantity: Number of items (must be > 0)
    /// - unitPrice: Price per unit (>= 0)
    /// 
    /// **Optional fields:**
    /// - productBatchId: Batch ID (if batch tracking is enabled)
    /// - supplierId: Supplier ID (required for Purchase transactions)
    /// - notes: Additional notes
    /// 
    /// **Transaction Types:**
    /// - 1: Purchase - Nhập hàng từ nhà cung cấp (requires supplierId)
    /// - 2: Sale - Bán hàng cho khách
    /// - 3: AdjustmentIn - Điều chỉnh tăng tồn kho
    /// - 4: AdjustmentOut - Điều chỉnh giảm tồn kho
    /// - 5: Damage - Hàng hư hỏng
    /// - 6: Expiry - Hàng hết hạn
    /// - 7: Return - Khách trả hàng
    /// - 8: SupplierReturn - Trả hàng cho nhà cung cấp
    /// 
    /// **Example request body (Purchase):**
    /// ```json
    /// {
    ///   "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "productBatchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    ///   "supplierId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
    ///   "transactionType": 1,
    ///   "quantity": 100,
    ///   "unitPrice": 12000,
    ///   "notes": "Nhập hàng đợt 1 tháng 1/2026"
    /// }
    /// ```
    /// 
    /// **Example request body (Sale):**
    /// ```json
    /// {
    ///   "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "productBatchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    ///   "transactionType": 2,
    ///   "quantity": 5,
    ///   "unitPrice": 15000,
    ///   "notes": "Bán lẻ"
    /// }
    /// ```
    /// 
    /// **Note:** For inbound transactions (Purchase, AdjustmentIn, Return), 
    /// batch quantity will be increased. For outbound transactions 
    /// (Sale, AdjustmentOut, Damage, Expiry, SupplierReturn), batch quantity will be decreased.
    /// </remarks>
    /// <param name="command">Transaction creation data</param>
    /// <returns>Created transaction</returns>
    [HttpPost]
    [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateStockTransaction([FromBody] CreateStockTransactionCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Không tìm thấy dữ liệu",
                    Detail = errorMessage,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi tạo giao dịch kho",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return CreatedAtAction(nameof(GetStockTransaction), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Create a purchase transaction (Nhập hàng)
    /// </summary>
    /// <remarks>
    /// Shortcut endpoint for creating a Purchase transaction.
    /// Equivalent to POST /api/stocktransactions with transactionType = 1
    /// </remarks>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest request)
    {
        var command = new CreateStockTransactionCommand
        {
            ProductId = request.ProductId,
            ProductBatchId = request.ProductBatchId,
            SupplierId = request.SupplierId,
            TransactionType = StockTransactionType.Purchase,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi nhập hàng",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });
        }

        return CreatedAtAction(nameof(GetStockTransaction), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Create a sale transaction (Bán hàng)
    /// </summary>
    /// <remarks>
    /// Shortcut endpoint for creating a Sale transaction.
    /// Equivalent to POST /api/stocktransactions with transactionType = 2
    /// </remarks>
    [HttpPost("sale")]
    [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
    {
        var command = new CreateStockTransactionCommand
        {
            ProductId = request.ProductId,
            ProductBatchId = request.ProductBatchId,
            TransactionType = StockTransactionType.Sale,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi bán hàng",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });
        }

        return CreatedAtAction(nameof(GetStockTransaction), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Create an adjustment transaction (Điều chỉnh tồn kho)
    /// </summary>
    /// <remarks>
    /// Shortcut endpoint for creating adjustment transactions.
    /// </remarks>
    [HttpPost("adjustment")]
    [ProducesResponseType(typeof(StockTransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateAdjustmentRequest request)
    {
        var transactionType = request.IsIncrease
            ? StockTransactionType.AdjustmentIn
            : StockTransactionType.AdjustmentOut;

        var command = new CreateStockTransactionCommand
        {
            ProductId = request.ProductId,
            ProductBatchId = request.ProductBatchId,
            TransactionType = transactionType,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi điều chỉnh tồn kho",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });
        }

        return CreatedAtAction(nameof(GetStockTransaction), new { id = result.Value!.Id }, result.Value);
    }
}

#region Request DTOs

/// <summary>
/// Request model for creating a purchase transaction
/// </summary>
public record CreatePurchaseRequest
{
    public Guid ProductId { get; init; }
    public Guid? ProductBatchId { get; init; }
    public Guid SupplierId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request model for creating a sale transaction
/// </summary>
public record CreateSaleRequest
{
    public Guid ProductId { get; init; }
    public Guid? ProductBatchId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request model for creating an adjustment transaction
/// </summary>
public record CreateAdjustmentRequest
{
    public Guid ProductId { get; init; }
    public Guid? ProductBatchId { get; init; }
    public bool IsIncrease { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Notes { get; init; }
}

#endregion
