using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Filters;
using SSMI.Models;
using System.Security.Claims;

namespace SSMI.Controllers
{
    [JwtCookieAuthorize("Usuario", "Pasajero")]
    public class UsuarioController : Controller
    {
        
        private readonly IConfiguration _configuracion;

        public UsuarioController(IConfiguration config)
        {
            _configuracion = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Perfil()
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(correo) && !string.IsNullOrWhiteSpace(idUsuario) && idUsuario.Contains('@'))
            {
                correo = idUsuario;
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                return RedirectToAction("Index", "Home");
            }

            var conStr = _configuracion.GetConnectionString("StringCONSQLocal");
            if (string.IsNullOrWhiteSpace(conStr))
            {
                return RedirectToAction("Index", "Home");
            }

            var cons = new ConsultaUsuario();
            var usuario = cons.ConsultarPasajeroPerfil(correo, conStr)
                          ?? cons.ConsultarUsuario(correo, conStr)
                          ?? new Usuario { Correo = correo };

            if (string.IsNullOrWhiteSpace(usuario.ID) && !string.IsNullOrWhiteSpace(idUsuario))
            {
                usuario.ID = idUsuario;
            }

            if (string.IsNullOrWhiteSpace(usuario.Rol))
            {
                usuario.Rol = User.FindFirst(ClaimTypes.Role)?.Value;
            }

            return View(usuario);
        }

        public IActionResult Configuracion()
        {
            return View();
        }
        public IActionResult MejorRuta()
        {
            return View();
        }
        public IActionResult Comentarios()
        {
            return View();
        }
        public IActionResult Ayuda()
        {
            return View();
        }
        public IActionResult AcercaDeNosotros()
        {
            return View();
        }
        public IActionResult TrayectoIniciado()
        {
            return View();
        }
        public IActionResult OtrasUnidades()
        {
            return View();
        }

    }
}
