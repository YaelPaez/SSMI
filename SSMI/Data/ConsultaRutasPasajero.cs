using Microsoft.Data.SqlClient;
using SSMI.Models;
using System.Data;

namespace SSMI.Data
{
    /// <summary>
    /// Clase para consultar rutas de autobús para pasajeros
    /// Ejecuta SP y convierte resultados a modelos
    /// </summary>
    public class ConsultaRutasPasajero
    {
        /// <summary>
        /// Calcula la mejor ruta en autobús desde un origen a un destino
        /// Utiliza el SP: spCalcularRutaAutobusConUnaVarianteInvolucrada
        /// </summary>
        /// <param name="latInicio">Latitud del punto de origen</param>
        /// <param name="lonInicio">Longitud del punto de origen</param>
        /// <param name="latFin">Latitud del punto de destino</param>
        /// <param name="lonFin">Longitud del punto de destino</param>
        /// <param name="cadenaConexion">Cadena de conexión a la BD</param>
        /// <returns>Lista de instrucciones de ruta en autobús</returns>
        public List<InstruccionesModel> ConsultarRutaAutobus(
            decimal latInicio,
            decimal lonInicio,
            decimal latFin,
            decimal lonFin,
            string cadenaConexion)
        {
            var instrucciones = new List<InstruccionesModel>();

            try
            {
                Console.WriteLine($"🔍 Iniciando consulta de ruta autobús");
                Console.WriteLine($"   Inicio: ({latInicio}, {lonInicio})");
                Console.WriteLine($"   Fin: ({latFin}, {lonFin})");

                using (SqlConnection con = new SqlConnection(cadenaConexion))
                {
                    con.Open();
                    Console.WriteLine("✅ Conexión abierta");

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "spCalcularRutaAutobusConUnaVarianteInvolucrada";
                        com.CommandTimeout = 30;

                        // Parámetros del SP
                        com.Parameters.AddWithValue("@LatInicio", latInicio);
                        com.Parameters.AddWithValue("@LonInicio", lonInicio);
                        com.Parameters.AddWithValue("@LatFin", latFin);
                        com.Parameters.AddWithValue("@LonFin", lonFin);

                        Console.WriteLine("✅ Parámetros configurados, ejecutando SP...");

                        using (SqlDataReader reader = com.ExecuteReader())
                        {
                            int filas = 0;
                            while (reader.Read())
                            {
                                filas++;
                                try
                                {
                                    var instruccion = new InstruccionesModel
                                    {
                                        // Propiedades comunes
                                        Tipo = "AUTOBUS",
                                        IndicacionTexto = ObtenerTextoInstruccion(reader["Instruccion"]?.ToString()),
                                        Distancia = reader["DistanciaTramo"] != DBNull.Value 
                                            ? Convert.ToDecimal(reader["DistanciaTramo"]) 
                                            : 0,
                                        Tiempo = reader["TiempoSeg"] != DBNull.Value 
                                            ? Convert.ToDecimal(reader["TiempoSeg"]) 
                                            : 0,
                                        PosicionLat = reader["Lat"] != DBNull.Value 
                                            ? Convert.ToDecimal(reader["Lat"]) 
                                            : 0,
                                        PosicionLon = reader["Lon"] != DBNull.Value 
                                            ? Convert.ToDecimal(reader["Lon"]) 
                                            : 0,

                                        // Propiedades específicas de AUTOBUS
                                        Estado = reader["Instruccion"]?.ToString(),
                                        IdParada = reader["IdParada"]?.ToString() ?? "",
                                        IdRutaVariante = reader["IdRutaVariante"] != DBNull.Value 
                                            ? (Guid)reader["IdRutaVariante"] 
                                            : Guid.Empty,
                                        SecuenciaRuta = reader["SecuenciaRuta"] != DBNull.Value 
                                            ? Convert.ToInt32(reader["SecuenciaRuta"]) 
                                            : 0
                                    };

                                    instrucciones.Add(instruccion);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"⚠️ Error procesando fila {filas}: {ex.Message}");
                                }
                            }

                            Console.WriteLine($"✅ SP ejecutado: {filas} filas leídas");
                        }
                    }
                }

                if (instrucciones.Count == 0)
                {
                    Console.WriteLine("⚠️ No se encontraron rutas disponibles");
                }
                else
                {
                    Console.WriteLine($"✅ Ruta consultada exitosamente: {instrucciones.Count} instrucciones");
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"❌ Error SQL: {sqlEx.Message}");
                Console.WriteLine($"   Número: {sqlEx.Number}");
                Console.WriteLine($"   LineNumber: {sqlEx.LineNumber}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ConsultarRutaAutobus: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                throw;
            }

            return instrucciones;
        }

        /// <summary>
        /// Obtiene el texto descriptivo para la instrucción
        /// </summary>
        private string ObtenerTextoInstruccion(string estado)
        {
            return estado switch
            {
                "SUBIR" => "Sube aquí al autobús",
                "BAJAR" => "Baja aquí del autobús",
                "SEGUIR" => "Continúa en el autobús",
                _ => "Sigue las indicaciones"
            };
        }

        /// <summary>
        /// Crea una instrucción de caminata desde datos de OSRM
        /// </summary>
        /// <param name="distanciaMetros">Distancia en metros</param>
        /// <param name="tiempoSegundos">Tiempo en segundos</param>
        /// <param name="latitud">Latitud de la ubicación</param>
        /// <param name="longitud">Longitud de la ubicación</param>
        /// <param name="instruccion">Instrucción detallada</param>
        /// <param name="nombreCalle">Nombre de la calle</param>
        /// <returns>Instrucción de caminata</returns>
        public InstruccionesModel CrearInstruccionCaminata(
            decimal distanciaMetros,
            decimal tiempoSegundos,
            decimal latitud,
            decimal longitud,
            string instruccion,
            string nombreCalle = "")
        {
            return new InstruccionesModel
            {
                // Propiedades comunes
                Tipo = "CAMINAR",
                IndicacionTexto = $"🚶 Camina {distanciaMetros:F0}m",
                Distancia = distanciaMetros,
                Tiempo = tiempoSegundos,
                PosicionLat = latitud,
                PosicionLon = longitud,

                // Propiedades específicas de CAMINAR
                DistanciaCaminata = distanciaMetros,
                TiempoCaminataSeg = tiempoSegundos,
                InstruccionDetallada = instruccion,
                NombreCalle = nombreCalle
            };
        }

        /// <summary>
        /// Combina instrucciones de caminata (OSRM) con la ruta en autobús
        /// </summary>
        /// <param name="instruccionesCaminataAlOrigen">Caminata al autobús</param>
        /// <param name="instruccionesAutobus">Ruta en autobús</param>
        /// <param name="instruccionesCaminataAlDestino">Caminata desde el autobús</param>
        /// <returns>Lista completa de instrucciones (caminar + autobus + caminar)</returns>
        public List<InstruccionesModel> CombinarInstrucciones(
            List<InstruccionesModel> instruccionesCaminataAlOrigen,
            List<InstruccionesModel> instruccionesAutobus,
            List<InstruccionesModel> instruccionesCaminataAlDestino)
        {
            var rutaCompleta = new List<InstruccionesModel>();

            // 1. Añadir caminata al origen
            if (instruccionesCaminataAlOrigen != null && instruccionesCaminataAlOrigen.Count > 0)
            {
                rutaCompleta.AddRange(instruccionesCaminataAlOrigen);
                Console.WriteLine($"✅ Añadidas {instruccionesCaminataAlOrigen.Count} instrucciones de caminata al origen");
            }

            // 2. Añadir ruta en autobús
            if (instruccionesAutobus != null && instruccionesAutobus.Count > 0)
            {
                rutaCompleta.AddRange(instruccionesAutobus);
                Console.WriteLine($"✅ Añadidas {instruccionesAutobus.Count} instrucciones de autobús");
            }

            // 3. Añadir caminata al destino
            if (instruccionesCaminataAlDestino != null && instruccionesCaminataAlDestino.Count > 0)
            {
                rutaCompleta.AddRange(instruccionesCaminataAlDestino);
                Console.WriteLine($"✅ Añadidas {instruccionesCaminataAlDestino.Count} instrucciones de caminata al destino");
            }

            Console.WriteLine($"✅ Ruta completa generada: {rutaCompleta.Count} instrucciones totales");
            return rutaCompleta;
        }

        /// <summary>
        /// DTO para respuesta de ruta completa
        /// </summary>
        public class RutaCompletoDto
        {
            public List<InstruccionesModel> Instrucciones { get; set; }
            public decimal DistanciaTotal { get; set; }
            public decimal TiempoTotalSegundos { get; set; }
            public decimal TiempoTotalMinutos { get; set; }
            public string ResumenRuta { get; set; }
            public int TotalPasos { get; set; }
        }

        /// <summary>
        /// Genera un resumen de la ruta completa
        /// </summary>
        public RutaCompletoDto GenerarResumenRuta(List<InstruccionesModel> instrucciones)
        {
            if (instrucciones == null || instrucciones.Count == 0)
            {
                return new RutaCompletoDto
                {
                    Instrucciones = new List<InstruccionesModel>(),
                    DistanciaTotal = 0,
                    TiempoTotalSegundos = 0,
                    TiempoTotalMinutos = 0,
                    ResumenRuta = "Sin ruta disponible",
                    TotalPasos = 0
                };
            }

            var distanciaTotal = instrucciones.Sum(x => x.Distancia);
            var tiempoTotalSegundos = instrucciones.Where(x => x.Tipo == "AUTOBUS").Sum(x => x.TiempoAcumuladoSeg) +
                                      instrucciones.Where(x => x.Tipo == "CAMINAR").Sum(x => x.TiempoCaminataSeg);
            var tiempoTotalMinutos = tiempoTotalSegundos / 60m;

            var caminataTotal = instrucciones.Where(x => x.Tipo == "CAMINAR").Sum(x => x.Distancia);
            var autobusCount = instrucciones.Count(x => x.Tipo == "AUTOBUS");
            var caminataCount = instrucciones.Count(x => x.Tipo == "CAMINAR");

            var resumen = $"🚶 Camina {caminataTotal:F0}m → 🚌 Autobus ({autobusCount} paradas) → 🚶 Camina final. " +
                         $"Total: {distanciaTotal:F0}m en {tiempoTotalMinutos:F1} min";

            return new RutaCompletoDto
            {
                Instrucciones = instrucciones,
                DistanciaTotal = distanciaTotal,
                TiempoTotalSegundos = tiempoTotalSegundos,
                TiempoTotalMinutos = tiempoTotalMinutos,
                ResumenRuta = resumen,
                TotalPasos = instrucciones.Count
            };
        }
    }
}

