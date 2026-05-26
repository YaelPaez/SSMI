using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Models;

namespace SSMI.Controllers
{
    public class DespachadorController : Controller
    {

        private readonly ConsultaHistorico _consultaHistorico;
        private readonly string _cadenaConexion;

        // El constructor recibe la configuración global del proyecto de manera automática
        public DespachadorController(IConfiguration configuration)
        {
            _consultaHistorico = new ConsultaHistorico();
            // Jala la cadena de conexión compartida por tu compañero
            _cadenaConexion = configuration.GetConnectionString("StringCONSQLocal");
        }

        // GET: DespachadorController
        public ActionResult Index()
        {
            return View();
        }

        // GET: DespachadorController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DespachadorController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DespachadorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DespachadorController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DespachadorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DespachadorController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DespachadorController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Perfil()
        {
            return View();
        }

        public ActionResult HistoricoDeRegistros(string buscarConductor, DateTime? inicioFecha, DateTime? finFecha, string buscarEstado)
        {

            // Validación de fechas por si el usuario pone un rango incorrecto
            if (inicioFecha.HasValue && finFecha.HasValue)
            {
                if (inicioFecha > finFecha)
                {
                    ViewBag.Error = "La fecha inicial no puede ser mayor a la final.";
                    return View(new List<RegistroHistoricoModel>());
                }
            }

            // Consultamos al servicio en Data pasándole la conexión y los filtros seleccionados
            List<RegistroHistoricoModel> registros = _consultaHistorico.ObtenerHistoricoFiltrado(
                _cadenaConexion,
                buscarConductor,
                inicioFecha,
                finFecha,
                buscarEstado
            );

            // Retorna la vista y le inyecta la lista real de registros
            return View(registros);
        }
    
    }
}
