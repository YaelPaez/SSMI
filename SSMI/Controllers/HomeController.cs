using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SSMI.Funciones;
using SSMI.Models;

namespace SSMI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
       

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult EquipoDesarrollo()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contacto()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Contacto(Contacto modelo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Completa todos los campos.";
                return View(modelo);
            }

            try
            {
                Correo correo = new Correo();

                // correo para code.revolution

                string titulo = "Nuevo mensaje de contacto";

                string cuerpo = $@"
<div style='
    font-family: Segoe UI, sans-serif;
    background: #f4f6f9;
    padding: 30px;
'>

    <div style='
        max-width: 600px;
        margin: auto;
        background: white;
        border-radius: 12px;
        padding: 30px;
        border: 1px solid #e5e7eb;
    '>

        <h2 style='
            color: #0d1b2a;
            margin-bottom: 25px;
        '>
            Nuevo mensaje de contacto
        </h2>

        <p>
            <strong>Nombre:</strong>
            {modelo.Nombre}
        </p>

        <p>
            <strong>Correo:</strong>
            {modelo.Correo}
        </p>

        <div style='
            margin-top: 25px;
            padding: 20px;
            background: #f9fafb;
            border-left: 4px solid #00d4ff;
            border-radius: 8px;
        '>

            <strong>Mensaje:</strong>

            <br><br>

            {modelo.Mensaje}

        </div>

    </div>

</div>";

                await correo.EnviarCorreoSMTP(
                    "Administrador",
                    "code.revolution08@gmail.com",
                    titulo,
                    cuerpo
                );

                // correo digirido para el usuario quien envio el correo

                string tituloUsuario = "Hemos recibido tu mensaje - SSMI";

                string cuerpoUsuario = $@"
        <div style='background:#0d1b2a;padding:40px;font-family:Segoe UI,sans-serif;color:white;'>

            <div style='max-width:600px;margin:auto;background:rgba(255,255,255,0.05);
            border-radius:20px;padding:40px;border:1px solid rgba(255,255,255,0.08);'>

                <h1 style='color:#00d4ff;text-align:center;'>
                    ¡Mensaje enviado correctamente!
                </h1>

                <p style='font-size:16px;line-height:1.8;'>

                    Hola <strong>{modelo.Nombre}</strong>,
                    <br><br>

                    Hemos recibido tu mensaje correctamente y nuestro equipo
                    intentará ponerse en contacto contigo lo antes posible.

                    <br><br>

                    Gracias por interesarte en <strong>SSMI</strong>.

                </p>

                <div style='margin-top:30px;padding:20px;border-radius:15px;
                background:rgba(0,212,255,0.08);
                border:1px solid rgba(0,212,255,0.25);'>

                    <strong>Tu mensaje:</strong>

                    <br><br>

                    {modelo.Mensaje}

                </div>

            </div>

        </div>";

                await correo.EnviarCorreoSMTP(
                    modelo.Nombre,
                    modelo.Correo,
                    tituloUsuario,
                    cuerpoUsuario
                );

                ViewBag.Exito = "Mensaje enviado correctamente.";

                ModelState.Clear();

                return View();
            }
            catch (Exception)
            {
                ViewBag.Error = "Ocurrió un error al enviar el mensaje.";
                return View(modelo);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
