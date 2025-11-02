using Asp.Versioning;
using EduCare.Application.Features.Core.AcademicYearManagement.Commands;
using EduCare.Application.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduCare.CoreApi.Controllers.Core;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class AcademicYearsController(IMediator mediator) : ControllerBase
{
    [MapToApiVersion("1.0")]
    [HttpPost]
    public async Task<IActionResult> CreateAcademicYear([FromBody] CreateAcademicYearRequestDto request)
    {
        var command = new CreateAcademicYearCommand(
            request.Name,
            request.Code,
            request.StartDate,
            request.EndDate,
            request.SchoolId,
            request.IsCurrent);

        var result = await mediator.Send(command);

        //if (result.IsSuccess)
        //{
        //    var response = result.Value.ToCreateResponse();

        //    // Return the result with the success message
        //    return Ok(Result<CreateAcademicYearResponseDto>.Succeeded(response, result.Message));
        //}

        return HandleErrorResult(result);
    }

    private IActionResult HandleErrorResult<T>(Result<T> result)
    {
        var errorResponse = Result<T>.Failed(result.Error, result.Message);

        return result.Error.Type switch
        {
            ErrorType.Validation => BadRequest(errorResponse),
            ErrorType.NotFound => NotFound(errorResponse),
            ErrorType.Conflict => Conflict(errorResponse),
            _ => StatusCode(500, errorResponse)
        };
    }
}