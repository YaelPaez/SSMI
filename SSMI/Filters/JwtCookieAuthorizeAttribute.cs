using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SSMI.Services;

namespace SSMI.Filters;

public class JwtCookieAuthorizeAttribute : TypeFilterAttribute
{
    public JwtCookieAuthorizeAttribute(params string[] roles)
        : base(typeof(JwtCookieAuthorizeFilter))
    {
        Arguments = new object[] { roles };
    }
}

public class JwtCookieAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthService _auth;
    private readonly string[] _roles;

    public JwtCookieAuthorizeFilter(IAuthService auth, string[] roles)
    {
        _auth = auth;
        _roles = roles ?? Array.Empty<string>();
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!_auth.IsAuthenticated)
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
            return Task.CompletedTask;
        }

        if (_roles.Length > 0)
        {
            var currentRole = _auth.GetCurrentRole();
            var ok = !string.IsNullOrWhiteSpace(currentRole) &&
                     _roles.Any(r => string.Equals(r, currentRole, StringComparison.OrdinalIgnoreCase));

            if (!ok)
            {
                var (controller, action) = _auth.GetHomeRouteForRole(currentRole);
                context.Result = new RedirectToActionResult(action, controller, null);
            }
        }

        return Task.CompletedTask;
    }
}
