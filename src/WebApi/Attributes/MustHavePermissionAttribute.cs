using EduCare.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace EduCare.CoreApi.Attributes
{
    public class MustHavePermissionAttribute : AuthorizeAttribute
    {
        public MustHavePermissionAttribute(string feature, string action)
            => Policy = AppPermission.NameFor(feature, action);        
    }
}
