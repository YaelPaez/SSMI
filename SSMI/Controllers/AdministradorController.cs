using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Funciones;
using SSMI.Models;
using SSMI.Models.ViewModels;
using System.Text;

namespace SSMI.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly ConsultasRutasAdm _consultasRutasAdm;
        private readonly IConfiguration _configuration;
        private readonly ConsultaConductor _consultaConductor;

        public AdministradorController(ConsultasRutasAdm consultasRutasAdm, IConfiguration configuration)
        {
            _consultasRutasAdm = consultasRutasAdm;
            _configuration = configuration;
            _consultaConductor = new ConsultaConductor();
        }
        // GET: Administrador/Index (Redirecciona a Paradas por defecto)
        public ActionResult Index()
        {
            return RedirectToAction(nameof(Paradas));
        }

        // ══ GESTIÓN DE PARADAS ══
        // GET: Administrador/Paradas
        public ActionResult Paradas()
        {
            return View();
        }

        // POST: Administrador/Paradas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearParada(IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de creación de parada
                return RedirectToAction(nameof(Paradas));
            }
            catch
            {
                return View(nameof(Paradas));
            }
        }

        // POST: Administrador/Paradas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarParada(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de edición de parada
                return RedirectToAction(nameof(Paradas));
            }
            catch
            {
                return View(nameof(Paradas));
            }
        }

        // POST: Administrador/Paradas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarParada(int id)
        {
            try
            {
                // TODO: Implementar lógica de eliminación de parada
                return RedirectToAction(nameof(Paradas));
            }
            catch
            {
                return View(nameof(Paradas));
            }
        }

        // ══ GESTIÓN DE CAMIONES ══
        // GET: Administrador/Camiones
        public ActionResult Camiones()
        {
            return View();
        }

        // POST: Administrador/Camiones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCamion(IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de creación de camión
                return RedirectToAction(nameof(Camiones));
            }
            catch
            {
                return View(nameof(Camiones));
            }
        }

        // POST: Administrador/Camiones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCamion(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de edición de camión
                return RedirectToAction(nameof(Camiones));
            }
            catch
            {
                return View(nameof(Camiones));
            }
        }

        // POST: Administrador/Camiones/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCamion(int id)
        {
            try
            {
                // TODO: Implementar lógica de eliminación de camión
                return RedirectToAction(nameof(Camiones));
            }
            catch
            {
                return View(nameof(Camiones));
            }
        }

        // ══ GESTIÓN DE CONDUCTORES ══
        // GET: Administrador/Conductores/Index
        public ActionResult Conductores()
        {
            return RedirectToAction(nameof(ConductoresIndex));
        }

        // GET: Administrador/ConductoresIndex (Listar)
        public ActionResult ConductoresIndex()
        {
            // Recuperamos tu cadena de conexión exacta
            var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;

            // Consultamos la lista de la BD usando el método corregido
            List<Conductor> listaConductores = _consultaConductor.ObtenerConductoresAdmin(cadenaConexion);

            // Retornamos la vista pasando los datos de los conductores
            return View(listaConductores);
        }

        // GET: Administrador/Conductores/Create
        public ActionResult ConductoresCreate()
        {
            return View();
        }

        // POST: Administrador/Conductores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConductoresCreate(string nombre, string email)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "El nombre y el correo electrónico son campos obligatorios.";
                return View();
            }

            try
            {
                var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;

                // 1. Generar la contraseña temporal aleatoria
                string contrasenaTemporal = GenerarContrasenaAleatoria(8);

                // 2. Registrar en la Base de Datos mediante la capa Data
                bool registrado = _consultaConductor.PreRegistrarConductor(nombre, email, contrasenaTemporal, cadenaConexion);

                if (registrado)
                {
                    // 3. ENVIAR CORREO ELECTRÓNICO (MailKit integrado con tu clase Correo)
                    string asunto = "Bienvenido a SSMI - Credenciales de Acceso";
                    string mensaje = $@"
                <h3>¡Hola, {nombre}!</h3>
                <p>Has sido registrado como conductor en el Sistema de Seguimiento de Movilidad Integrada (SSMI).</p>
                <p>Usa las siguientes credenciales temporales para ingresar al sistema:</p>
                <ul>
                    <li><strong>Usuario:</strong> {email}</li>
                    <li><strong>Contraseña Temporal:</strong> {contrasenaTemporal}</li>
                </ul>
                <p><em>Al iniciar sesión por primera vez deberás completar tus datos personales.</em></p>";

                    // Instanciamos tu clase e invocamos el método con tus parámetros específicos
                    Correo correoServicio = new Correo();
                    await correoServicio.EnviarCorreoSMTP(nombre, email, asunto, mensaje);

                    TempData["Exito"] = $"El conductor {nombre} fue invitado con éxito. Se envió la contraseña temporal a su correo.";
                    return RedirectToAction(nameof(ConductoresIndex));
                }
                else
                {
                    ViewBag.Error = "No se pudo completar el pre-registro en la base de datos.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error inesperado: " + ex.Message;
                return View();
            }
        }

        // GET: Administrador/Conductores/Edit/5
        public ActionResult ConductoresEdit(int id)
        {
            // TODO: Traer datos del conductor desde la BD
            return View("ConductoresEdit");
        }

        // POST: Administrador/Conductores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductoresEdit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de edición de conductor
                return RedirectToAction(nameof(ConductoresIndex));
            }
            catch
            {
                return View("Conductores/Edit");
            }
        }

        // GET: Administrador/Conductores/Delete/5
        public ActionResult ConductoresDelete(int id)
        {
            // TODO: Traer datos del conductor desde la BD
            return View("Conductores/Delete");
        }

        // POST: Administrador/Conductores/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductoresDelete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de eliminación de conductor
                return RedirectToAction(nameof(ConductoresIndex));
            }
            catch
            {
                return View("Conductores/Delete");
            }
        }

        // ══ MONITOREO EN TIEMPO REAL ══
        // GET: Administrador/Monitoreo
        public ActionResult Monitoreo()
        {
            return View();
        }

        public ActionResult Ruta()
        {
            try
            {
                // Obtener ID del administrador desde la sesión o JWT
                var idAdminString = User?.FindFirst("IdAdmin")?.Value ?? HttpContext.Session.GetString("IdAdmin");

                if (string.IsNullOrWhiteSpace(idAdminString) || !Guid.TryParse(idAdminString, out var idAdmin))
                {
                    return RedirectToAction("IniciarSesion", "Login");
                }

                var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;
                var (rutas, variantes) = _consultasRutasAdm.ConsultarRutasYVariantesAdm(idAdmin, cadenaConexion);

                var viewModel = new RutaViewModelAdm
                {
                    Rutas = rutas,
                    Variantes = variantes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR en Ruta: " + ex.Message);
                ViewBag.Error = "Error al cargar rutas y variantes";
                return View(new RutaViewModelAdm());
            }
        }

        private string GenerarContrasenaAleatoria(int longitud)
        {
            const string caracteres = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            StringBuilder resultado = new StringBuilder();
            Random rnd = new Random();

            while (0 < longitud--)
            {
                resultado.Append(caracteres[rnd.Next(caracteres.Length)]);
            }

            return resultado.ToString();
        }

    }
}
