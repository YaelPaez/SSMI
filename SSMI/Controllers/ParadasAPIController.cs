using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Models;

namespace SSMI.Controllers
{
    public class ParadasAPIController : Controller
    {
        private readonly IConfiguration _configuracion;
        private readonly String _connStr;

        public ParadasAPIController(IConfiguration config)
        {
            _configuracion = config;
            _connStr = _configuracion.GetConnectionString("StringCONSQLocal");
        }


        [HttpPost]
        public IActionResult ObtenerParadasCercanas([FromBody] UbicacionRequest request)
        {
            ConsultasParadas cons = new ConsultasParadas();
            var paradas = cons.ConsultarParadas(
                _connStr,
                request.Lat,
                request.Lon
                );

            return Json(paradas);
        }

        [HttpGet]
        public IActionResult ObtenerTodasLasParadas()
        {
            ConsultasParadas cons = new ConsultasParadas();
            var paradas = cons.ConsultarTodasParadas(
                _connStr
                );

            return Json(paradas);
        }
    }
}
