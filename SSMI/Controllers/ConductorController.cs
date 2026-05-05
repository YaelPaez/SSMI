using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Models;
using SSMI.Models.ViewModels;

namespace SSMI.Controllers
{
    public class ConductorController : Controller
    {
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

            // Validar archivo
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
            return View(model);

        }

    }
}
