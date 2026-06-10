namespace SSMI.Models
{
    /// <summary>
    /// Modelo unificado para instrucciones de ruta
    /// Soporta dos tipos:
    /// 1. AUTOBUS: instrucciones del recorrido en autobús (SUBIR, SEGUIR, BAJAR)
    /// 2. CAMINAR: instrucciones de caminata desde OSRM
    /// </summary>
    public class InstruccionesModel
    {
        // ?? PROPIEDADES COMUNES ??
        /// <summary>
        /// Tipo de instrucción: "AUTOBUS" o "CAMINAR"
        /// </summary>
        public string? Tipo { get; set; }

        /// <summary>
        /// Descripción de la instrucción
        /// </summary>
        public string? IndicacionTexto { get; set; }

        /// <summary>
        /// Distancia en metros
        /// </summary>
        public decimal Distancia { get; set; }

        /// <summary>
        /// Tiempo en segundos
        /// </summary>
        public decimal Tiempo { get; set; }

        /// <summary>
        /// Coordenadas de la instrucción
        /// </summary>
        public decimal PosicionLat { get; set; }
        public decimal PosicionLon { get; set; }

        // ?? PROPIEDADES ESPECÍFICAS PARA AUTOBUS ??
        /// <summary>
        /// Estado de la parada (SUBIR, BAJAR, SEGUIR) - Solo para AUTOBUS
        /// </summary>
        public string? Estado { get; set; }

        /// <summary>
        /// ID de la parada - Solo para AUTOBUS
        /// </summary>
        public string? IdParada { get; set; }

        /// <summary>
        /// ID de la variante/ruta - Solo para AUTOBUS
        /// </summary>
        public Guid IdRutaVariante { get; set; }

        /// <summary>
        /// Secuencia en la ruta - Solo para AUTOBUS
        /// </summary>
        public int SecuenciaRuta { get; set; }

        /// <summary>
        /// Distancia acumulada hasta este punto - Solo para AUTOBUS
        /// </summary>
        public decimal DistanciaAcumulada { get; set; }

        /// <summary>
        /// Tiempo acumulado en segundos - Solo para AUTOBUS
        /// </summary>
        public decimal TiempoAcumuladoSeg { get; set; }

        /// <summary>
        /// Tiempo acumulado en minutos - Solo para AUTOBUS
        /// </summary>
        public decimal TiempoAcumuladoMin { get; set; }

        // ?? PROPIEDADES ESPECÍFICAS PARA CAMINAR (OSRM) ??
        /// <summary>
        /// Distancia de caminata - Solo para CAMINAR
        /// </summary>
        public decimal DistanciaCaminata { get; set; }

        /// <summary>
        /// Tiempo de caminata en segundos - Solo para CAMINAR
        /// </summary>
        public decimal TiempoCaminataSeg { get; set; }

        /// <summary>
        /// Instrucción detallada de caminata - Solo para CAMINAR
        /// </summary>
        public string? InstruccionDetallada { get; set; }

        /// <summary>
        /// Nombre de la calle - Solo para CAMINAR
        /// </summary>
        public string? NombreCalle { get; set; }
    }
}
