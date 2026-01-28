using InventoryManagement.Application.Commands.CreateSupplier;
using InventoryManagement.Application.Commands.DeleteSupplier;
using InventoryManagement.Application.Commands.UpdateSupplier;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Queries.GetSupplier;
using InventoryManagement.Application.Queries.GetSuppliers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BiBoGC.Controllers;

/// <summary>
/// API Controller for Supplier management operations
/// Handles CRUD operations for suppliers in the inventory system
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of suppliers
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of suppliers with optional search and filter functionality.
    /// 
    /// **Search behavior:**
    /// - Searches across supplier name, contact person, phone, and address
    /// - Case-insensitive matching using PostgreSQL ILike
    /// 
    /// **Filtering:**
    /// - isActive: Filter by active/inactive status (default: true - only active)
    /// 
    /// **Sorting:**
    /// - sortBy: Field to sort by (Name, CreatedAt)
    /// - sortDescending: Sort order (default: false - ascending)
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Items per page (1-100, default: 10)</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="isActive">Filter by active status (default: true)</param>
    /// <param name="sortBy">Sort field (Name, CreatedAt)</param>
    /// <param name="sortDescending">Sort descending (default: false)</param>
    /// <returns>Paginated list of suppliers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<SuppliersDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = true,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetSuppliersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a single supplier by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific supplier including recent transactions.
    /// 
    /// **Response includes:**
    /// - Basic supplier info: id, name, contactName, contactPhone, address
    /// - Status: isActive
    /// - Recent transactions: up to 5 most recent stock transactions
    /// </remarks>
    /// <param name="id">Supplier ID (GUID)</param>
    /// <returns>Supplier details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplier(Guid id)
    {
        var query = new GetSupplierQuery { SupplierId = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy nhà cung cấp",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new supplier
    /// </summary>
    /// <remarks>
    /// Creates a new supplier in the system.
    /// 
    /// **Required fields:**
    /// - name: Supplier name (max 200 characters)
    /// 
    /// **Optional fields:**
    /// - contactPerson: Contact person name (max 100 characters)
    /// - phoneNumber: Phone number (max 20 characters)
    /// - address: Address (max 500 characters)
    /// 
    /// **Example request body:**
    /// ```json
    /// {
    ///   "name": "Công ty TNHH ABC",
    ///   "contactPerson": "Nguyễn Văn A",
    ///   "phoneNumber": "0901234567",
    ///   "address": "123 Đường ABC, Quận 1, TP.HCM"
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Supplier creation data</param>
    /// <returns>Created supplier</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi tạo nhà cung cấp",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });

        return CreatedAtAction(nameof(GetSupplier), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update an existing supplier
    /// </summary>
    /// <remarks>
    /// Updates supplier information.
    /// 
    /// **Updatable fields:**
    /// - name: Supplier name
    /// - contactName: Contact person name
    /// - contactPhone: Phone number
    /// - address: Address
    /// - isActive: Active status
    /// 
    /// **Example request body:**
    /// ```json
    /// {
    ///   "name": "Công ty TNHH ABC Updated",
    ///   "contactName": "Nguyễn Văn B",
    ///   "contactPhone": "0909876543",
    ///   "address": "456 Đường XYZ, Quận 2, TP.HCM",
    ///   "isActive": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Supplier ID to update</param>
    /// <param name="command">Update data</param>
    /// <returns>Updated supplier</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierCommand command)
    {
        // Ensure ID matches
        if (id != command.Id && command.Id != Guid.Empty)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "ID không khớp",
                Detail = "ID trong URL và body phải giống nhau.",
                Status = StatusCodes.Status400BadRequest
            });

        var updateCommand = new UpdateSupplierCommand
        {
            Id = id,
            Name = command.Name,
            ContactName = command.ContactName,
            ContactPhone = command.ContactPhone,
            Address = command.Address,
            IsActive = command.IsActive
        };

        var result = await _mediator.Send(updateCommand);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("không tìm thấy", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ProblemDetails
                {
                    Title = "Không tìm thấy nhà cung cấp",
                    Detail = errorMessage,
                    Status = StatusCodes.Status404NotFound
                });

            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi cập nhật nhà cung cấp",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a supplier (soft delete)
    /// </summary>
    /// <remarks>
    /// Performs a soft delete on the supplier.
    /// The supplier will be marked as deleted but data is preserved in the database.
    /// Deleted suppliers will not appear in search results.
    /// </remarks>
    /// <param name="id">Supplier ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        var command = new DeleteSupplierCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy nhà cung cấp",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        return NoContent();
    }

    /// <summary>
    /// Activate a supplier
    /// </summary>
    /// <param name="id">Supplier ID to activate</param>
    /// <returns>Activated supplier</returns>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateSupplier(Guid id)
    {
        // Get current supplier first
        var getQuery = new GetSupplierQuery { SupplierId = id };
        var getResult = await _mediator.Send(getQuery);

        if (!getResult.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy nhà cung cấp",
                Detail = getResult.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        var supplier = getResult.Value!;
        var updateCommand = new UpdateSupplierCommand
        {
            Id = id,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            ContactPhone = supplier.ContactPhone,
            Address = supplier.Address,
            IsActive = true
        };

        var result = await _mediator.Send(updateCommand);
        return Ok(result.Value);
    }

    /// <summary>
    /// Deactivate a supplier
    /// </summary>
    /// <param name="id">Supplier ID to deactivate</param>
    /// <returns>Deactivated supplier</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateSupplier(Guid id)
    {
        // Get current supplier first
        var getQuery = new GetSupplierQuery { SupplierId = id };
        var getResult = await _mediator.Send(getQuery);

        if (!getResult.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy nhà cung cấp",
                Detail = getResult.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        var supplier = getResult.Value!;
        var updateCommand = new UpdateSupplierCommand
        {
            Id = id,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            ContactPhone = supplier.ContactPhone,
            Address = supplier.Address,
            IsActive = false
        };

        var result = await _mediator.Send(updateCommand);
        return Ok(result.Value);
    }
}