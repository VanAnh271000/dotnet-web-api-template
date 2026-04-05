using Application.Interfaces.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Application.Services.Identity
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(
            IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                context.Fail();
                return;
            }

            var hasPermission = _permissionService.HasPermissionAsync(userId, requirement.Permission);

            if (hasPermission.Data)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
