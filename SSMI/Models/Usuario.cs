using System.ComponentModel.DataAnnotations;

namespace SSMI.Models
{
    public class Usuario
    {
        public string? ID { get; set; }

        public string? Rol { get; set; }

        [Required(ErrorMessage = "Ingresa tu nombre")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$",
            ErrorMessage = "El nombre solo puede contener letras")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Ingresa tus apellidos")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$",
            ErrorMessage = "Los apellidos solo pueden contener letras")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Ingresa una contraseña")]
        [StringLength(12,
            MinimumLength = 6,
            ErrorMessage = "La contraseña debe tener entre 6 y 12 caracteres")]
        [RegularExpression(@"^(?=.*[\W_]).+$",
            ErrorMessage = "La contraseña debe contener al menos un carácter especial $, #, @")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Ingresa un correo válido")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Ingresa un número telefónico")]
        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "El teléfono debe contener exactamente 10 números")]
        public string Numtelefono { get; set; }

        [Required(ErrorMessage = "Selecciona un género")]
        public string Genero { get; set; }

        [Required(ErrorMessage = "Selecciona tu fecha de nacimiento")]
        public DateOnly FechaNacimiento { get; set; }

        public string? Discapacidad { get; set; }
    }
}