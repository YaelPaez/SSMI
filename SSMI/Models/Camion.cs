using SSMI.Data;
using System.ComponentModel.DataAnnotations;

namespace SSMI.Models
{
    public class Camion
    {
        public Guid IdCamion { get; set; }
        public Guid? Ruta { get; set; }
        public Guid? Conductor { get; set; }
        public string Placas { get; set; }
        public int Kilometraje { get; set; }
        public string Economico { get; set; }
        public string Capacidad { get; set; }
        public string NombreConductor { get; set; }
        // CORRECCIÓN: El nombre visible de la ruta debe ser una cadena de texto
        public string? NombreRuta { get; set; }


    }
}
