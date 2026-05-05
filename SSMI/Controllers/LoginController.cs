using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Funciones;
using SSMI.Models;

namespace SSMI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuracion;
        public LoginController(IConfiguration config)
        {
            _configuracion = config;
        }
        public IActionResult IniciarSesion()
        {
            Captcha captcha = new Captcha();
            InicioSesionYRegistro Obj = new InicioSesionYRegistro()
            {
                Captcha = new DatosCaptcha(),
            };
            Obj.Captcha.CaptchaGenerado = captcha.GenerarCaptcha();
            return View(Obj);
        }

        [HttpPost]
        public IActionResult IniciarSesion(InicioSesionYRegistro datos)
        {
            Captcha cap = new Captcha();
            Contrasena ctr = new Contrasena();
            ConsultaUsuario cons = new ConsultaUsuario();
            string conStr = _configuracion.GetConnectionString("StringCONSQLocal");
            Usuario usuario = datos.Usuario;

            Usuario usrEnc = cons.ConsultarUsuario(usuario.Correo, conStr);

            if (datos.Captcha.CaptchaGenerado != datos.Captcha.Captcha)
            {
                ViewBag.Error = "Validar Captcha";
                datos.Captcha.CaptchaGenerado = cap.GenerarCaptcha();
                return View(datos);
            }

            if (usrEnc == null)
            {
                ViewBag.Error = "usuario no encontrado";
                datos.Captcha.CaptchaGenerado = cap.GenerarCaptcha();
                return View();
            }

            if (!ctr.CompararContrsanas(usuario.Contrasena, usrEnc.Contrasena))
            {
                ViewBag.Error = "Credenciales incorrectas";
                datos.Captcha.CaptchaGenerado = cap.GenerarCaptcha();
                return View();
            }
            else
            {
                ViewBag.Error = "Sesion Iniciada";
                return RedirectToAction("Index", "Usuario");
            }


        }
        public IActionResult Registro()
        {
            Captcha captcha = new Captcha();
            InicioSesionYRegistro Obj = new InicioSesionYRegistro()
            {
                Captcha = new DatosCaptcha(),
            };
            Obj.Captcha.CaptchaGenerado = captcha.GenerarCaptcha();
            return View(Obj);
        }
        [HttpPost]
        public IActionResult Registro(InicioSesionYRegistro datos)
        {
            Contrasena ctr = new Contrasena();
            ConsultaUsuario cons = new ConsultaUsuario();
            string conStr = _configuracion.GetConnectionString("StringCONSQLocal");
            Usuario usuarioN = datos.Usuario;


            if (datos.Captcha.CaptchaGenerado != datos.Captcha.Captcha)
            {
                ViewBag.Error = "Validar Captcha";
                Captcha cap = new Captcha();
                datos.Captcha.CaptchaGenerado = cap.GenerarCaptcha();
                return View(datos);
            }

            string hash = ctr.EncriptarContrasena(usuarioN.Contrasena);
            usuarioN.Contrasena = hash;
            usuarioN.Rol = "Usuario";


            cons.RegistrarUsuario(usuarioN, conStr);



            return RedirectToAction("IniciarSesion", "Login");
        }


        public IActionResult CerrarSesion()
        {
            return View();
        }
    }
}
