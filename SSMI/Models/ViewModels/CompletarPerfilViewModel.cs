using System.ComponentModel.DataAnnotations;

namespace SSMI.Models.ViewModels
{
    public class CompletarPerfilViewModel
    {
        public string Email { get; set; } // Solo lectura en la vista

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        // ^(?!\s*$) evita que el campo sea solo espacios. Al final se limpia con .Trim() en el controlador.
        [RegularExpression(@"^(?!\s*$)[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El apellido paterno solo puede contener letras y no puede estar vacío.")]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPaterno { get; set; }

        [Required(ErrorMessage = "El apellido materno es obligatorio")]
        [RegularExpression(@"^(?!\s*$)[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El apellido materno solo puede contener letras y no puede estar vacío.")]
        [Display(Name = "Apellido Materno")]
        public string ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "La empresa es obligatoria")]
        [RegularExpression(@"^(?!\s*$).+$", ErrorMessage = "La empresa no puede contener solo espacios en blanco.")]
        [Display(Name = "Empresa")]
        public string Empresa { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(12, ErrorMessage = "La contraseña debe tener entre 6 y 12 caracteres.", MinimumLength = 6)]
        // ^(?=\S+$) -> PROHÍBE estrictamente cualquier espacio en blanco en toda la cadena
        // (?=.*[#\!$%\^&\*\(\)_\+\-\=\[\]\{\};':""\\|,\.<>\/\?]) -> Exige al menos un carácter especial
        [RegularExpression(@"^(?=\S+$)(?=.*[#\!$%\^&\*\(\)_\+\-\=\[\]\{\};':""\\|,\.<>\/\?]).+$", ErrorMessage = "La contraseña no puede contener espacios y debe incluir al menos un carácter especial.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaContrasena { get; set; }

        [Required(ErrorMessage = "Confirmar la contraseña es obligatorio")]
        [StringLength(12, ErrorMessage = "La confirmación debe tener entre 6 y 12 caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; }
    }
}