using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Models;
using SSMI.Services;

namespace SSMI.Controllers
{
    public class ParadasAPIController : Controller
    {
        private readonly IRutasService _rutasService;

        public ParadasAPIController(IRutasService rutasService)
        {
            _rutasService = rutasService;
        }


        [HttpPost]
        public IActionResult ObtenerParadasCercanas([FromBody] UbicacionRequest request)
        {
            var paradas = _rutasService.ObtenerParadasCercanas(request);
            return Json(paradas);
        }

        [HttpGet]
        public IActionResult ObtenerTodasLasParadas()
        {
            var paradas = _rutasService.ObtenerTodasLasParadas();
            return Json(paradas);
        }
    }
}
