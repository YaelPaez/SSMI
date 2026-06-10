using SSMI.Data;
using SSMI.Services;
using SSMI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ?? CONFIGURAR CORS ??
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()          // Permitir cualquier origen
              .AllowAnyMethod()          // Permitir cualquier método (GET, POST, etc)
              .AllowAnyHeader();         // Permitir cualquier header
    });

    options.AddPolicy("AllowSpecific", policy =>
    {
        policy.WithOrigins(
                "https://ssmi.site",
                "http://localhost:3035",
                "http://localhost:3000",
                "http://127.0.0.1:3035"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key ?? string.Empty))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies[jwt.CookieName];
                if (!string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Application services
builder.Services.AddTransient<ConsultasParadas>();
builder.Services.AddScoped<IRutasService, RutasService>();
builder.Services.AddScoped<ConsultaRutasPasajero>();

builder.WebHost.UseUrls("http://0.0.0.0:3035");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// ?? USAR CORS ??
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ?? MAPEAR RUTAS ??
app.MapControllers();  // Para rutas de atributos [Route]

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
