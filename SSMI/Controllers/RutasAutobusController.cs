using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Models;
using SSMI.Services;
using System.Threading.Tasks;

namespace SSMI.Controllers
{
    /// <summary>
    /// API para calcular y consultar rutas de autobús para pasajeros
    /// Combina instrucciones de caminata (OSRM) con rutas en autobús (BD)
    /// </summary>
    
    public class RutasAutobusController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public RutasAutobusController(IConfiguration config)
        {
            _configuracion = config;
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerRuta([FromBody] RutaRequestModel Req) 
        {
            string conStr = _configuracion.GetConnectionString("StringCONSQLocal");
            ConsultaRutasPasajero consultaRutas = new ConsultaRutasPasajero();
            GraphhopperService graphhopperService = new GraphhopperService();
            List<InstruccionesModel> InstruccionesAutobus = consultaRutas.ConsultarRutaAutobus(Req.LatInicio, Req.LonInicio, Req.LatFin, Req.LonFin, conStr);

            var PrimerParada = InstruccionesAutobus.First();
            var UltimaParada = InstruccionesAutobus.Last();

            List<InstruccionesModel> CaminataInicio = await graphhopperService.ObtenerCaminata(Req.LatInicio, Req.LonInicio, PrimerParada.PosicionLat, PrimerParada.PosicionLon);

            List<InstruccionesModel> CaminataFin = await graphhopperService.ObtenerCaminata(UltimaParada.PosicionLat, UltimaParada.PosicionLon, Req.LatFin, Req.LonFin);

            List<InstruccionesModel> RutaCompleta = new List<InstruccionesModel>();

            RutaCompleta.AddRange(CaminataInicio);
            RutaCompleta.AddRange(InstruccionesAutobus);
            RutaCompleta.AddRange(CaminataFin);

            for (int i = 0; i < RutaCompleta.Count; i++)
            {
                var elementoActual = RutaCompleta[i];

                
                if (i-1 >= 0){ 
                var elementoAnterior = RutaCompleta[i-1];

                elementoActual.DistanciaAcumulada = elementoAnterior.DistanciaAcumulada + elementoActual.Distancia;
                elementoActual.TiempoAcumuladoSeg = elementoAnterior.TiempoAcumuladoSeg + elementoActual.Tiempo;
                elementoActual.TiempoAcumuladoMin = elementoActual.TiempoAcumuladoSeg / 60;
                }
                else
                {
                    elementoActual.DistanciaAcumulada = elementoActual.Distancia;
                    elementoActual.TiempoAcumuladoSeg = elementoActual.Tiempo;
                    elementoActual.TiempoAcumuladoMin = elementoActual.TiempoAcumuladoSeg / 60;

                }

                elementoActual.SecuenciaRuta = i+1;
            }

            return Ok(RutaCompleta);
        }
    }
}
