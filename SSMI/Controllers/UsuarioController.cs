using Microsoft.AspNetCore.Mvc;
using SSMI.Models;

namespace SSMI.Controllers
{
    public class UsuarioController : Controller
    {
        
        private readonly IConfiguration _configuracion;

        public UsuarioController(IConfiguration config)
        {
            _configuracion = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Perfil()
        {
            return View();
        }

        public IActionResult Configuracion()
        {
            return View();
        }
        public IActionResult MejorRuta()
        {
            return View();
        }
        public IActionResult Comentarios()
        {
            return View();
        }
        public IActionResult Ayuda()
        {
            return View();
        }
        public IActionResult AcercaDeNosotros()
        {
            return View();
        }
        public IActionResult TrayectoIniciado()
        {
            return View();
        }
        public IActionResult OtrasUnidades()
        {
            return View();
        }

    }
}
