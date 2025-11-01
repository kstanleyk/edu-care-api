using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduCare.CoreApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ApiControllerBase<T> : ControllerBase
    {
        private ISender _sender;

        public ISender MediatorSender => _sender ??= HttpContext.RequestServices.GetService<ISender>();

        protected async Task<IActionResult> GetActionResult(Func<Task<IActionResult>> codeToExecute)
        {
            return await codeToExecute.Invoke();
        }
    }
}
