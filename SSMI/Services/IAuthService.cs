using SSMI.Models;

namespace SSMI.Services;

public interface IAuthService
{
    Task SignInAsync(Usuario usuario);
    Task SignOutAsync();

    bool IsAuthenticated { get; }
    string? GetCurrentRole();

    (string controller, string action) GetHomeRouteForRole(string? role);
}
