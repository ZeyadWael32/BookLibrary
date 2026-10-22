using BookLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookLibrary.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ToErrorResponse<T>(ServiceResult<T> result)
    {
        var status = result.ErrorType switch
        {
            ServiceErrorType.NotFound => StatusCodes.Status404NotFound,
            ServiceErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return StatusCode(status, new ProblemDetails
        {
            Status = status,
            Title = result.ErrorTitle,
            Detail = result.ErrorDetail
        });
    }
}