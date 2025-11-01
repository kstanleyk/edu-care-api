using EduCare.Application.Features.Auth.Permission.Queries;
using EduCare.CoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduCare.CoreApi.Controllers.Auth;

public class PermissionsController(CurrentUserService currentUserService) : ApiControllerBase<PermissionsController>
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = currentUserService.UserId;
        if (userId == null) return Unauthorized();

        var permissions = await MediatorSender.Send(new PermissionsQuery
        {
            UserId = userId
        });

        return Ok(permissions);
    }
}