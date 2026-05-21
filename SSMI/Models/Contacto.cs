using System.ComponentModel.DataAnnotations;

namespace SSMI.Models
{
    public class Contacto
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required]
        public string Mensaje { get; set; }
    }
}