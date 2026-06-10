using SSMI.Models;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SSMI.Services
{
    public class GraphhopperService
    {
        public async Task<List<InstruccionesModel>> ObtenerCaminata(decimal LatInicio, decimal LonInicio, decimal LatFin ,decimal LonFin)
        {
            using HttpClient cliente = new HttpClient();
            List<InstruccionesModel> Res = new List<InstruccionesModel>();
            try
            {
                string latInicio = LatInicio.ToString(CultureInfo.InvariantCulture);
                string lonInicio = LonInicio.ToString(CultureInfo.InvariantCulture);
                string latFin = LatFin.ToString(CultureInfo.InvariantCulture);
                string lonFin = LonFin.ToString(CultureInfo.InvariantCulture);

                string url =
                $"https://ssmi.site/graphhopper/route" +
                $"?profile=foot" +
                $"&ch.disable=true" +
                $"&instructions=true" +
                $"&points_encoded=false" +
                $"&point={latInicio},{lonInicio}" +
                $"&point={latFin},{lonFin}";

                string resultadoGH = await cliente.GetStringAsync(url);

                JsonDocument resultadoJson = JsonDocument.Parse(resultadoGH);

                var coordenadas = resultadoJson.RootElement.GetProperty("paths")[0].GetProperty("points").GetProperty("coordinates");


                foreach (var instruccion in resultadoJson.RootElement.GetProperty("paths")[0].GetProperty("instructions").EnumerateArray())
                {
                    var intervalo = instruccion.GetProperty("interval");

                    int indiceInicio = intervalo[0].GetInt32();

                    decimal lon = Convert.ToDecimal(coordenadas[indiceInicio][0].GetDouble());

                    decimal lat = Convert.ToDecimal(coordenadas[indiceInicio][1].GetDouble());

                    Res.Add(new InstruccionesModel
                    {
                        Tipo = "CAMINAR",
                        IndicacionTexto = instruccion.GetProperty("text").GetString(),
                        InstruccionDetallada = instruccion.GetProperty("text").GetString(),
                        Distancia = Convert.ToDecimal(instruccion.GetProperty("distance").GetDouble()),
                        DistanciaCaminata = Convert.ToDecimal(instruccion.GetProperty("distance").GetDouble()),
                        Tiempo = Convert.ToDecimal(instruccion.GetProperty("time").GetDouble() / 1000),
                        TiempoCaminataSeg = Convert.ToDecimal(instruccion.GetProperty("time").GetDouble() / 1000),
                        Estado = null,
                        IdParada = null,
                        PosicionLat = lat,
                        PosicionLon = lon
                    });
                }
                
            } catch (Exception ex)
            {
                Console.WriteLine(ex); 
            }
            return Res;
        }
    }
}
