using InventoryManagement.Application.Commands.CreateCategory;
using InventoryManagement.Application.Commands.DeleteCategory;
using InventoryManagement.Application.Commands.UpdateCategory;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Queries.GetCategories;
using InventoryManagement.Application.Queries.GetCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiBoGC.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger;
    private readonly IMediator _mediator;

    public CategoriesController(ILogger<CategoriesController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] bool includeInactive = false,
        [FromQuery] Guid? parentCategoryId = null)
    {
        var query = new GetCategoriesQuery
        {
            IncludeInactive = includeInactive,
            ParentCategoryId = parentCategoryId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var query = new GetCategoryQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy danh mục",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status404NotFound
            });

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi tạo danh mục",
                Detail = result.Errors.FirstOrDefault(),
                Status = StatusCodes.Status400BadRequest
            });

        return CreatedAtAction(nameof(GetCategory), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        // Manually set the Id property instead of using 'with' expression
        command.Id = id;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("Không tìm thấy"))
                return NotFound(new ProblemDetails
                {
                    Title = "Không tìm thấy danh mục",
                    Detail = errorMessage,
                    Status = StatusCodes.Status404NotFound
                });

            return BadRequest(new ValidationProblemDetails
            {
                Title = "Lỗi cập nhật danh mục",
                Detail = errorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var command = new DeleteCategoryCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
            if (errorMessage.Contains("Danh mục này có danh mục con, hãy xóa danh mục con trước"))
                return BadRequest(new ProblemDetails
                {
                    Title = "Yêu cầu không hợp lệ",
                    Detail = errorMessage,
                    Status = StatusCodes.Status400BadRequest
                });

            return NotFound(new ProblemDetails
            {
                Title = "Không tìm thấy danh mục",
                Detail = errorMessage,
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }
}