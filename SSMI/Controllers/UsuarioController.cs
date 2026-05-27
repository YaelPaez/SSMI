using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Filters;
using SSMI.Models;
using System.Security.Claims;

namespace SSMI.Controllers
{
    //[JwtCookieAuthorize("Usuario")] // <--- SI NO HAS INICIADO SESION NO TE DEJA ENTRAR A LAS VISTAS DEL USUARIO
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarPerfil(Usuario modeloModificado, IFormCollection form)
        {
            var correoUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(correoUsuario))
            {
                return RedirectToAction("Index", "Home");
            }

            modeloModificado.Correo = correoUsuario;

            // ─── VALIDACIONES DEL SERVIDOR ───

            // 1. Validar Teléfono (que sean 10 números)
            if (string.IsNullOrWhiteSpace(modeloModificado.Numtelefono) ||
                modeloModificado.Numtelefono.Length != 10 ||
                !System.Text.RegularExpressions.Regex.IsMatch(modeloModificado.Numtelefono, @"^[0-9]+$"))
            {
                // Si no cumple, cancelamos y recargamos la vista sin guardar
                return RedirectToAction("Perfil");
            }

            // 2. Validar Mayoría de Edad y fechas futuras
            // Nota: Como DateOnly puede ser un poco estricto al mapear, calculamos la edad así:
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var edad = hoy.Year - modeloModificado.FechaNacimiento.Year;

            // Ajuste por si no ha pasado su cumpleaños este año
            if (modeloModificado.FechaNacimiento > hoy.AddYears(-edad)) edad--;

            if (modeloModificado.FechaNacimiento > hoy || edad < 18)
            {
                return RedirectToAction("Perfil");
            }

            // 3. Validar que los campos de nombres no vayan vacíos o con números
            if (string.IsNullOrWhiteSpace(modeloModificado.Nombres) ||
                string.IsNullOrWhiteSpace(modeloModificado.Apellidos) ||
                System.Text.RegularExpressions.Regex.IsMatch(modeloModificado.Nombres, @"[0-9]") ||
                System.Text.RegularExpressions.Regex.IsMatch(modeloModificado.Apellidos, @"[0-9]"))
            {
                return RedirectToAction("Perfil");
            }

            // ─── FIN DE VALIDACIONES (SI PASA, SE GUARDA) ───

            var conStr = _configuracion.GetConnectionString("StringCONSQLocal");
            var cons = new ConsultaUsuario();
            bool exito = cons.ActualizarDatosUsuario(modeloModificado, conStr);

            return RedirectToAction("Perfil");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarComentario(int Calificacion, string Detalle)
        {
            // CASO 1: ¿El sistema no encuentra tu sesión o tu correo?
            var correoUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(correoUsuario))
            {
                // Agregamos un mensaje para saber si fue por esto
                TempData["ErrorComentario"] = "Error de seguridad: No se encontró el correo de la sesión activa.";
                return RedirectToAction("Comentarios"); // Te regresa a comentarios para que veas el error
            }

            // CASO 2: ¿El texto del comentario llegó vacío a C#?
            if (string.IsNullOrWhiteSpace(Detalle))
            {
                TempData["ErrorComentario"] = "Error: El comentario llegó vacío al servidor. Revisa el atributo 'name' en tu HTML.";
                return RedirectToAction("Comentarios");
            }

            var conStr = _configuracion.GetConnectionString("StringCONSQLocal");
            var cons = new ConsultaUsuario();

            bool guardado = cons.InsertarComentario(correoUsuario, Calificacion, Detalle, conStr);

            // CASO 3: ¿La consulta SQL falló o devolvió false?
            if (guardado)
            {
                TempData["MensajeComentario"] = "Comentario enviado exitosamente, agradecemos su opinion.";
            }
            else
            {
                TempData["ErrorComentario"] = "Hubo un problema al conectar con el servidor. Por favor, inténtalo más tarde.";
            }

            return RedirectToAction("Comentarios");
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
