using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Models;
using SSMI.Models.ViewModels;
using SSMI.Data;

namespace SSMI.Controllers
{
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
