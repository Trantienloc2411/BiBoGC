using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Queries.GetProductVariants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiBoGC.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProductVariantsController(IMediator mediator)
    : ControllerBase
{
    /// <summary>
    /// Get a paginated list of product variants
    /// </summary>
    /// <remarks>
    /// Retrieves a paginated list of product variants with optional search functionality.
    /// **Search behavior:**
    ///  - Searches across variant name and SKU Unique, Barcode, and ProductName
    ///  - Case-insensitive matching using PostgreSQL ILike
    ///  - Results sorted alphabetically by variant name
    ///
    /// **Pagination:**
    ///  - Page numbers are 1-based
    ///  - Default page size is 10, maximum is 100
    ///  - Response includes total count and page metadata
    ///
    /// **Response fields:**
    ///  - items: Array of variant objects
    ///  - pageNumber: Current page (1-based)
    ///  - pageSize: Items per page
    ///  - totalCount: Total items across all pages
    ///  - totalPages: Total number of pages
    ///  - hasPrevious: Boolean indicating if previous page exists
    ///  - hasNext: Boolean indicating if next page exists
    ///
    /// **Example usage:**
    /// GET /api/productvariants?pageNumber=1&pageSize=10&searchTerm=coca
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based, default: 1) </param>
    /// <param name="pageSize">Item per page (1-100, default 10)</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns paginated product list</response>
    /// <response code="400">Invalid pagination parameters</response>
    [HttpGet]
    [Authorize(Roles = "Administrator,Seller")]
    [ProducesResponseType(typeof(IEnumerable<ProductVariantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductVariants(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null
    )
    {
        var query = new GetProductVariantsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchString = searchTerm
        };

        var result = await mediator.Send(query);
        return Ok(result);
    }
}