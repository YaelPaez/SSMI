using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SSMI.Models;

namespace SSMI.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions _jwt;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IOptions<JwtOptions> jwtOptions, IHttpContextAccessor httpContextAccessor)
    {
        _jwt = jwtOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? GetCurrentRole() => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public (string controller, string action) GetHomeRouteForRole(string? role)
    {
        role ??= string.Empty;

        return role switch
        {
            "Pasajero" => ("Usuario", "Index"),
            "Conductor" => ("Conductor", "Index"),
            "Despachador" => ("Despachador", "Index"),
            "Administrador" or "Adminitrador" => ("Adminitrador", "Index"),
            _ => ("Home", "Index")
        };
    }

    public Task SignInAsync(Usuario usuario)
    {
        var http = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No hay HttpContext disponible");

        if (string.IsNullOrWhiteSpace(_jwt.Key))
        {
            throw new InvalidOperationException("Configura Jwt:Key en appsettings.json");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, string.IsNullOrWhiteSpace(usuario.ID) ? usuario.Correo : usuario.ID),
            new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}".Trim()),
            new Claim(ClaimTypes.Email, usuario.Correo ?? string.Empty),
            new Claim(ClaimTypes.Role, usuario.Rol ?? string.Empty),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        http.Response.Cookies.Append(
            _jwt.CookieName,
            tokenValue,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = http.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = new DateTimeOffset(expires)
            });

        return Task.CompletedTask;
    }

    public Task SignOutAsync()
    {
        var http = _httpContextAccessor.HttpContext;
        if (http != null)
        {
            http.Response.Cookies.Delete(_jwt.CookieName);
        }

        return Task.CompletedTask;
    }
}
