using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Filters;
using SSMI.Models;
using SSMI.Models.ViewModels;

namespace SSMI.Controllers
{
    [JwtCookieAuthorize("Conductor")]
    public class ConductorController : Controller
    {

        private readonly IConfiguration _configuration;



        public ConductorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: ConductorController1
        public ActionResult Index()
        {
            return View();
        }

        // GET: Conductor/CompletarPerfil
        public ActionResult CompletarPerfil()
        {
            // Recuperamos el correo del usuario autenticado desde los Claims del JWT
            string? emailUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(emailUsuario))
            {
                return RedirectToAction("IniciarSesion", "Login");
            }

            // Preparamos el ViewModel enviándole el correo ya establecido
            var model = new CompletarPerfilViewModel
            {
                Email = emailUsuario
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompletarPerfil(CompletarPerfilViewModel model)
        {


            // 1. Forzamos a recuperar el correo real de la sesión del usuario autenticado
            string? emailUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            // 2. Se lo asignamos al modelo pase lo que pase
            model.Email = emailUsuario ?? string.Empty;

            // 3. LIMPIAMOS el error del Email en el ModelState para que ya no bloquee la validación
            ModelState.Remove(nameof(model.Email));

            // Ahora sí, evaluamos si los demás campos (apellidos, contraseña) están bien
            if (!ModelState.IsValid)
            {
                var errores = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                ViewBag.Error = "El formulario tiene errores de validación: " + errores;
                return View(model);
            }

            try
            {
                string conStr = _configuration.GetConnectionString("StringCONSQLocal");

                // Encriptar la nueva contraseña definitiva
                Funciones.Contrasena ctr = new Funciones.Contrasena();
                string contrasenaHash = ctr.EncriptarContrasena(model.NuevaContrasena);

                // Mandar a guardar a la base de datos usando el correo seguro de la sesión
                ConsultaConductor consulta = new ConsultaConductor();
                bool actualizado = consulta.ActualizarPerfilConductor(
                    model.Email,
                    model.ApellidoPaterno,
                    model.ApellidoMaterno,
                    model.Empresa,
                    contrasenaHash,
                    conStr
                );

                if (actualizado)
                {
                    TempData["Exito"] = "Perfil completado con éxito. Ahora estás activo en el sistema.";
                    return RedirectToAction("Index", "Conductor");
                }
                else
                {
                    ViewBag.Error = "No se pudo actualizar el perfil en la base de datos. Verifica si el usuario existe.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error inesperado al procesar tu solicitud: " + ex.Message;
                return View(model);
            }
        }

        public ActionResult PerfilConductor()
        {
            // 1. Recuperamos el correo del conductor autenticado
            string? emailUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(emailUsuario))
            {
                return RedirectToAction("IniciarSesion", "Login");
            }

            try
            {
                string conStr = _configuration.GetConnectionString("StringCONSQLocal");
                ConsultaConductor consulta = new ConsultaConductor();

                // 2. Buscamos con nuestra nueva consulta combinada (JOIN)
                CompletarPerfilViewModel? conductorDatos = consulta.ObtenerPerfilPorEmail(emailUsuario, conStr);

                if (conductorDatos == null)
                {
                    // Si por alguna razón el JOIN no encuentra coincidencia en tbConductores, pasamos al menos el correo
                    return View(new CompletarPerfilViewModel { Email = emailUsuario });
                }

                // 3. Enviamos los datos completos a la vista
                return View(conductorDatos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al mapear los datos del perfil: " + ex.Message;
                return View(new CompletarPerfilViewModel { Email = emailUsuario });
            }
        }

        public ActionResult MiUnidad()
        {
            return View();
        }


        public ActionResult Incidencias()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Incidencias(IncidenciaViewModel model)
        {
            if (model.Incidente == "otro" && string.IsNullOrWhiteSpace(model.Otro))
            {
                ModelState.AddModelError("Otro", "Debes especificar el incidente");
            }

            if (model.Evidencia != null)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(model.Evidencia.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("Evidencia", "Solo se permiten imágenes JPG o PNG");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? nombreArchivo = null;

            // Guardar imagen
            if (model.Evidencia != null)
            {
                string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/evidencias");

                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                nombreArchivo = Guid.NewGuid().ToString() +
                                Path.GetExtension(model.Evidencia.FileName);

                string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    model.Evidencia.CopyTo(stream);
                }
            }

            // Crear modelo
            IncidenciaModel incidencia = new IncidenciaModel
            {
                TipoIncidente = model.Incidente == "otro"
                    ? model.Otro
                    : model.Incidente,

                Descripcion = model.Descripcion,
                RutaEvidencia = nombreArchivo
            };

            // Guardar en BD
            ConsultaIncidencia consulta = new ConsultaIncidencia();

            string cadenaCon = _configuration.GetConnectionString("StringCONSQLocal");

            consulta.RegistrarIncidencia(incidencia, cadenaCon);

            TempData["Mensaje"] = "Incidencia enviada correctamente";

            return RedirectToAction("Incidencias");

        }

    }
}
