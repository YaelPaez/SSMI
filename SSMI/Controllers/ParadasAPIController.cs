using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Models;
using SSMI.Services;

namespace SSMI.Controllers
{
    [ApiController]
    [Route("api/paradas")]
    public class ParadasAPIController : ControllerBase
    {
        private readonly IRutasService _rutasService;

        public ParadasAPIController(IRutasService rutasService)
        {
            _rutasService = rutasService;
        }

        /// <summary>
        /// Obtiene paradas cercanas a una ubicación específica
        /// </summary>
        [HttpPost("obtener-cercanas")]
        public IActionResult ObtenerParadasCercanas([FromBody] UbicacionRequest request)
        {
            try
            {
                var paradas = _rutasService.ObtenerParadasCercanas(request);
                return Ok(paradas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las paradas disponibles
        /// </summary>
        [HttpGet("obtener-todas")]
        public IActionResult ObtenerTodasLasParadas()
        {
            try
            {
                var paradas = _rutasService.ObtenerTodasLasParadas();
                return Ok(paradas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
