using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Api.Extensions;

namespace RaceDay.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32(SessionKeys.UserId);
            var role = session.GetString(SessionKeys.Role);

            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "You must be logged in to access this resource."
                });
                return;
            }

            if (_roles.Length > 0 && (role == null || !_roles.Contains(role)))
            {
                context.Result = new ObjectResult(new
                {
                    message = $"Access denied. Required role: {string.Join(" or ", _roles)}"
                })
                { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}
