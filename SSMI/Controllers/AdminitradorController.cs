using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace SSMI.Controllers
{
    public class AdminitradorController : Controller
    {

        private readonly IConfiguration _configuration;

        public AdminitradorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly string _cadenaConexion;

        // 👈 El constructor recibe la configuración del archivo .json mediante Inyección de Dependencias
        public AdminitradorController(IConfiguration configuration)
        {
            // Jalamos el nombre exacto que pusiste en el appsettings.json
            _cadenaConexion = configuration.GetConnectionString("StringCONSQLocal");
        }
        // GET: Administrador/Index (Redirecciona a Paradas por defecto)
        public ActionResult Index()
        {
            return RedirectToAction(nameof(Paradas));
        }

        // ══ GESTIÓN DE PARADAS ══
        // GET: Administrador/Paradas
        public ActionResult Paradas()
        {
            return View();
        }

        // POST: Administrador/Paradas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearParada(IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de creación de parada
                return RedirectToAction(nameof(Paradas));
            }
            catch
            {
                return View(nameof(Paradas));
            }
        }

        // POST: Administrador/Paradas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarParada(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de edición de parada
                return RedirectToAction(nameof(Paradas));
            }
            catch
            {
                return View(nameof(Paradas));
            }
        }

        // POST: Administrador/Paradas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarParada(int id)
        {
            try
            {
                // TODO: Implementar lógica de eliminación de parada
                return RedirectToAction(nameof(Paradas));
            }
            catch
            {
                return View(nameof(Paradas));
            }
        }

        // ══ GESTIÓN DE CAMIONES ══
        // GET: Administrador/Camiones
        public ActionResult Camiones()
        {
            return View();
        }

        // POST: Administrador/Camiones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCamion(Guid Ruta, Guid Conductor, string Placas, int Kilometraje, string Economico, string Capacidad)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    SqlCommand comando = new SqlCommand("Sp_InsertarCamion", conexion);
                    comando.CommandType = CommandType.StoredProcedure;

                    // Pasamos los parámetros exactos al procedimiento almacenado
                    comando.Parameters.AddWithValue("@Ruta", Ruta);
                    comando.Parameters.AddWithValue("@Conductor", Conductor); // El Guid del conductor (IdUsuario)
                    comando.Parameters.AddWithValue("@Placas", Placas);
                    comando.Parameters.AddWithValue("@Kilometraje", Kilometraje);
                    comando.Parameters.AddWithValue("@Economico", Economico);
                    comando.Parameters.AddWithValue("@Capacidad", Capacidad);

                    conexion.Open();
                    comando.ExecuteNonQuery(); 
                }
                TempData["MensajeExito"] = "¡El camión se registró correctamente en el sistema!";
                return RedirectToAction(nameof(Camiones));
            }
            catch (SqlException ex)
            {
                
                if (ex.Number == 26 || ex.Number == 53 || ex.Number == 11001)
                {
                    TempData["MensajeError"] = "No se pudo establecer conexión con el servidor de movilidad. Por favor, verifica que la base de datos remota esté activa.";
                }
                else if (ex.Number == 2627 || ex.Number == 2601) // Error de llave duplicada
                {
                    TempData["MensajeError"] = "Error: Ya existe un camión registrado con ese número económico o placa.";
                }
                else
                {
                    TempData["MensajeError"] = "Ocurrió un problema interno en la base de datos al intentar registrar el camión.";
                }

                return RedirectToAction(nameof(Camiones));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error inesperado al procesar la solicitud. Inténtalo de nuevo.";
                return RedirectToAction(nameof(Camiones));
            }
        }

        // POST: Administrador/Camiones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCamion(Guid id, Guid Ruta, Guid Conductor, string Placas, int Kilometraje, string Economico, string Capacidad)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    SqlCommand comando = new SqlCommand("Sp_ModificarCamion", conexion);
                    comando.CommandType = CommandType.StoredProcedure;

                    // Pasamos los parámetros incluyendo el ID del camión a modificar
                    comando.Parameters.AddWithValue("@IdCamion", id);
                    comando.Parameters.AddWithValue("@Ruta", Ruta);
                    comando.Parameters.AddWithValue("@Conductor", Conductor);
                    comando.Parameters.AddWithValue("@Placas", Placas);
                    comando.Parameters.AddWithValue("@Kilometraje", Kilometraje);
                    comando.Parameters.AddWithValue("@Economico", Economico);
                    comando.Parameters.AddWithValue("@Capacidad", Capacidad);

                    conexion.Open();
                    comando.ExecuteNonQuery();
                }
                TempData["MensajeExito"] = "¡Los datos del camión se actualizaron correctamente!";
                return RedirectToAction(nameof(Camiones));
            }
            catch (SqlException ex)
            {
                if (ex.Number == 26 || ex.Number == 53 || ex.Number == 11001)
                {
                    TempData["MensajeError"] = "No se pudo conectar con el servidor para guardar los cambios. Verifica la red.";
                }
                else if (ex.Number == 2627 || ex.Number == 2601)
                {
                    TempData["MensajeError"] = "Error: El número económico o las placas ya están asignados a otro camión.";
                }
                else
                {
                    TempData["MensajeError"] = "Ocurrió un problema interno en la base de datos al intentar modificar el camión.";
                }

                return RedirectToAction(nameof(Camiones));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error inesperado al procesar la actualización.";
                return RedirectToAction(nameof(Camiones));
            }
        }

        // POST: Administrador/Camiones/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCamion(Guid id)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    SqlCommand comando = new SqlCommand("Sp_EliminarCamion", conexion);
                    comando.CommandType = CommandType.StoredProcedure;

                    // Para eliminar solo necesitamos el identificador único del camión
                    comando.Parameters.AddWithValue("@IdCamion", id);

                    conexion.Open();
                    comando.ExecuteNonQuery();
                }
                TempData["MensajeExito"] = "El camión fue removido del sistema con éxito.";
                return RedirectToAction(nameof(Camiones));
            }
            catch (SqlException ex)
            {
                if (ex.Number == 26 || ex.Number == 53 || ex.Number == 11001)
                {
                    TempData["MensajeError"] = "No se pudo conectar con el servidor para eliminar el registro.";
                }
                else if (ex.Number == 547) 
                {
                    TempData["MensajeError"] = "No se puede eliminar este camión porque actualmente está asignado a un viaje activo o historial de movilidad.";
                }
                else
                {
                    TempData["MensajeError"] = "Ocurrió un error interno en la base de datos al intentar eliminar el camión.";
                }

                return RedirectToAction(nameof(Camiones));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error inesperado al intentar eliminar el registro.";
                return RedirectToAction(nameof(Camiones));
            }
        }

        // ══ GESTIÓN DE CONDUCTORES ══
        // GET: Administrador/Conductores/Index
        public ActionResult Conductores()
        {
            return RedirectToAction("Index", "Conductores");
        }

        // GET: Administrador/Conductores/Index (Listar)
        public ActionResult ConductoresIndex()
        {
            // TODO: Traer lista de conductores desde la BD
            return View("ConductoresEdit");
        }

        // GET: Administrador/Conductores/Create
        public ActionResult ConductoresCreate()
        {
            return View("ConductoresEdit");
        }

        // POST: Administrador/Conductores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductoresCreate(IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de creación de conductor
                return RedirectToAction(nameof(ConductoresIndex));
            }
            catch
            {
                return View("Conductores/Create");
            }
        }

        // GET: Administrador/Conductores/Edit/5
        public ActionResult ConductoresEdit(int id)
        {
            // TODO: Traer datos del conductor desde la BD
            return View("ConductoresEdit");
        }

        // POST: Administrador/Conductores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductoresEdit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de edición de conductor
                return RedirectToAction(nameof(ConductoresIndex));
            }
            catch
            {
                return View("Conductores/Edit");
            }
        }

        // GET: Administrador/Conductores/Delete/5
        public ActionResult ConductoresDelete(int id)
        {
            // TODO: Traer datos del conductor desde la BD
            return View("Conductores/Delete");
        }

        // POST: Administrador/Conductores/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductoresDelete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Implementar lógica de eliminación de conductor
                return RedirectToAction(nameof(ConductoresIndex));
            }
            catch
            {
                return View("Conductores/Delete");
            }
        }

        // ══ MONITOREO EN TIEMPO REAL ══
        // GET: Administrador/Monitoreo
        public ActionResult Monitoreo()
        {
            return View();
        }

        // ══ HISTORIAL DE INCIDENCIAS ══
        public ActionResult HistorialIncidencias()
        {
            ConsultaIncidencia consulta = new ConsultaIncidencia();

            string cadenaCon =
                _configuration.GetConnectionString("StringCONSQLocal");

            List<IncidenciaModel> incidencias =
                consulta.ConsultarIncidencias(cadenaCon);

            return View(incidencias);
        }
    }
}
