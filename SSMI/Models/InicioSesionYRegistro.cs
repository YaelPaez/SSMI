using SSMI.Models.ViewModels;

namespace SSMI.Models
{
    public class InicioSesionYRegistro
    {
        public Usuario? Usuario { get; set; }

        public DatosCaptcha? Captcha { get; set; }
    }
}
