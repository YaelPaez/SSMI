using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Funciones;
using SSMI.Models;
using SSMI.Services;

namespace SSMI.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuracion;
        private readonly IAuthService _auth;

        public LoginController(IConfiguration config, IAuthService auth)
        {
            _configuracion = config;
            _auth = auth;
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
        public async Task<IActionResult> IniciarSesion(InicioSesionYRegistro datos)
        {
            Captcha cap = new Captcha();
            Contrasena ctr = new Contrasena();
            ConsultaUsuario cons = new ConsultaUsuario();
            string conStr = _configuracion.GetConnectionString("StringCONSQLocal");
            Usuario usuario = datos.Usuario;

            
            usuario.Correo = usuario.Correo?.Trim();

            if (string.IsNullOrWhiteSpace(usuario.Correo))
            {
                ViewBag.Error = "Ingresa un correo válido";
                datos.Captcha.CaptchaGenerado = cap.GenerarCaptcha();
                return View(datos);
            }

            Usuario? usrEnc = cons.ConsultarUsuario(usuario.Correo, conStr);

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
                return View(datos);
            }

            if (!ctr.CompararContrsanas(usuario.Contrasena, usrEnc.Contrasena))
            {
                ViewBag.Error = "Credenciales incorrectas";
                datos.Captcha.CaptchaGenerado = cap.GenerarCaptcha();
                return View(datos);
            }
            else
            {
                ViewBag.Error = "Sesion Iniciada";
                await _auth.SignInAsync(usrEnc);

                var (controller, action) = _auth.GetHomeRouteForRole(usrEnc.Rol);
                return RedirectToAction(action, controller);
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

            // VALIDAR MODELO
            if (!ModelState.IsValid)
            {
                Captcha capModelo = new Captcha();
                datos.Captcha.CaptchaGenerado = capModelo.GenerarCaptcha();

                return View(datos);
            }

            // VALIDAR MAYOR DE EDAD
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            if (usuarioN.FechaNacimiento > hoy)
            {
                ModelState.AddModelError("Usuario.FechaNacimiento",
                    "La fecha no puede ser futura");
            }
            else
            {
                int edad = hoy.Year - usuarioN.FechaNacimiento.Year;

                if (usuarioN.FechaNacimiento > hoy.AddYears(-edad))
                {
                    edad--;
                }

                if (edad < 18)
                {
                    ModelState.AddModelError("Usuario.FechaNacimiento",
                        "Debes ser mayor de 18 años");
                }
            }

            // VALIDAR CAPTCHA
            if (datos.Captcha.CaptchaGenerado != datos.Captcha.Captcha)
            {
                ModelState.AddModelError("Captcha.Captcha",
                    "Captcha incorrecto");
            }

            if (!ModelState.IsValid)
            {
                Captcha capError = new Captcha();
                datos.Captcha.CaptchaGenerado = capError.GenerarCaptcha();

                return View(datos);
            }

            // VALIDAR CORREO DUPLICADO
            if (cons.ExisteCorreo(usuarioN.Correo, conStr))
            {
                ModelState.AddModelError("Usuario.Correo",
                    "Este correo ya está registrado");

                Captcha capError = new Captcha();
                datos.Captcha.CaptchaGenerado = capError.GenerarCaptcha();

                return View(datos);
            }

            // ENCRIPTAR CONTRASEÑA
            string hash = ctr.EncriptarContrasena(usuarioN.Contrasena);
            usuarioN.Contrasena = hash;
            usuarioN.Rol = "Usuario";


            cons.RegistrarUsuario(usuarioN, conStr);



            return RedirectToAction("IniciarSesion", "Login");
        }


        public async Task<IActionResult> CerrarSesion()
        {
            await _auth.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
