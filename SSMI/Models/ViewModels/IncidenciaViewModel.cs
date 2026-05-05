using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SSMI.Models.ViewModels
{
    public class IncidenciaViewModel
    {
        [Required(ErrorMessage = "Selecciona un tipo de incidente")]
        public string Incidente { get; set; }

        public string? Otro { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MinLength(10, ErrorMessage = "Mínimo 10 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debes subir una imagen")]
        public IFormFile Evidencia { get; set; }
    }
}