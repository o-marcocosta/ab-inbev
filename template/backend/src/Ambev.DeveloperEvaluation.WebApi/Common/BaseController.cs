using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected Guid GetCurrentUserId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new NullReferenceException());

    protected string GetCurrentUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? throw new NullReferenceException();

    protected IActionResult Ok<T>(T data, string message = "") =>
            base.Ok(new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    protected IActionResult OkMessage(string message) =>
            base.Ok(new ApiResponse { Success = true, Message = message });

    protected IActionResult Created<T>(string actionName, object routeValues, T data, string message = "") =>
        base.CreatedAtAction(actionName, routeValues, new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    protected IActionResult BadRequest(string message) =>
        base.BadRequest(new ApiResponse { Message = message, Success = false });

    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(new ApiResponse { Message = message, Success = false });

    protected IActionResult OkPaginated<T>(PagedResult<T> page) =>
            base.Ok(new PaginatedResponse<T>
            {
                Data = page.Items,
                CurrentPage = page.Page,
                TotalPages = page.TotalPages,
                TotalItems = page.TotalCount,
                Success = true
            });
}
