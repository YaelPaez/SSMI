namespace SSMI.Models
{
    public class IncidenciaModel
    {
        public int Id { get; set; }
        public string TipoIncidente { get; set; }
        public string Descripcion { get; set; }
        public string? RutaEvidencia { get; set; } // Aquí guardaremos el texto (Ej: "foto_conductor.jpg")
        public DateTime FechaReporte { get; set; }
        public string Estado { get; set; }
    }
}
