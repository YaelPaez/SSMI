using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SSMI.Controllers
{
    public class DespachadorController : Controller
    {
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

        public ActionResult HistoricoDeRegistros()
        {
            return View();
        }
    }
}
