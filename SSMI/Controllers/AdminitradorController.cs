using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSMI.Data;
using SSMI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using SSMI.Funciones;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace SSMI.Controllers
{
    public class AdminitradorController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly string _cadenaConexion;
        private readonly ConsultaConductor _consultaConductor;


        public AdminitradorController(IConfiguration configuration)
        {
            _configuration = configuration;
            _cadenaConexion = configuration.GetConnectionString("StringCONSQLocal");
            _consultaConductor = new ConsultaConductor();

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
        // GET: Administrador/Camiones
        public ActionResult Camiones()
        {
            // 1. Creamos la lista utilizando el tipo exacto que espera tu vista
            List<Camion> listaCamiones = new List<Camion>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
                {
                    // Ajusta "Sp_ConsultarCamiones" al nombre exacto de tu Procedimiento Almacenado
                    using (SqlCommand comando = new SqlCommand("Sp_ListarCamionesConConductor", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        conexion.Open();

                        using (SqlDataReader dr = comando.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                // 2. Mapeamos las columnas de la Base de Datos a las propiedades de tu modelo Camion
                                Camion camion = new Camion
                                {
                                    IdCamion = Guid.Parse(dr["IdCamion"].ToString()),
                                    Placas = dr["Placas"].ToString(),
                                    Kilometraje = Convert.ToInt32(dr["Kilometraje"]),
                                    Economico = dr["Economico"].ToString(),
                                    Capacidad = dr["Capacidad"].ToString(),

                                    // Tu vista usa esta propiedad: @camion.NombreConductor
                                    // Asegúrate de que tu SP traiga esta columna haciendo un JOIN con la tabla de Usuarios/Conductores
                                    NombreConductor = dr["NombreConductor"] != DBNull.Value ? dr["NombreConductor"].ToString() : null
                                };

                                listaCamiones.Add(camion);
                            }
                        }
                    }

                    // 3. OPCIONAL: Cargar conductores reales para llenar el <select> de tu modal
                    List<dynamic> listaConductores = new List<dynamic>();
                    using (SqlCommand comandoCond = new SqlCommand("Sp_ListarCamionesConConductor", conexion)) // Ajusta el nombre de tu SP
                    {
                        comandoCond.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader drCond = comandoCond.ExecuteReader())
                        {
                            while (drCond.Read())
                            {
                                listaConductores.Add(new
                                {
                                    IdUsuario = Guid.Parse(drCond["IdUsuario"].ToString()),
                                    NombreCompleto = drCond["NombreCompleto"].ToString()
                                });
                            }
                        }
                    }
                    // Lo guardamos en ViewBag para que el modal lo dibuje dinámicamente
                    ViewBag.Conductores = listaConductores;
                }
            }
            catch (SqlException ex)
            {
                // Errores de red o conexión que ya controlabas en los POST
                if (ex.Number == 26 || ex.Number == 53 || ex.Number == 11001)
                {
                    TempData["MensajeError"] = "No se pudo establecer conexión con el servidor de movilidad. Verifica la red remota.";
                }
                else
                {
                    TempData["MensajeError"] = "Ocurrió un problema interno en la base de datos al intentar mostrar la lista el camión."; 
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error inesperado al cargar la lista de camiones: " + ex.Message;
            }

            // 4. Enviamos la lista tipada. Ahora coincide al 100% con @model IEnumerable<SSMI.Models.Camion>
            return View(listaCamiones);
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
                    TempData["MensajeError"] = ex.Message;
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
            return RedirectToAction(nameof(ConductoresIndex));
        }

        [HttpGet]
        public ActionResult ConductoresIndex()
        {
            var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;
            List<Conductor> listaConductores = _consultaConductor.ObtenerConductoresAdmin(cadenaConexion);
            return View("ConductoresEdit", listaConductores);
        }

        // GET: Administrador/Conductores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConductoresCreate(string nombre, string email)
        {
            // Si los campos fallan, necesitamos recargar la lista para no dejar la vista rota
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email))
            {
                var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;
                List<Conductor> listaConductores = _consultaConductor.ObtenerConductoresAdmin(cadenaConexion);

                ViewBag.Error = "El nombre y el correo electrónico son campos obligatorios.";

                // CORRECCIÓN: Retorna tu vista real con su lista obligatoria
                return View("ConductoresEdit", listaConductores);
            }


            try
            {
                var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;

                // ─── VALIDACIÓN DE CORREO DUPLICADO OPTIMIZADA ───
                if (_consultaConductor.ExisteCorreoConductor(email, cadenaConexion))
                {
                    // Guardamos el error de forma persistente para la redirección
                    TempData["MensajeError"] = $"No se pudo enviar la invitación. El correo electrónico '{email.Trim()}' ya pertenece a un conductor registrado.";

                    // Redireccionamos limpiamente para romper la ejecución y evitar el envío del correo
                    return RedirectToAction(nameof(ConductoresIndex));
                }

                // 1. Generar la contraseña temporal aleatoria (Limpia)
                string contrasenaTemporal = GenerarContrasenaAleatoria(8);

                // 2. ENVIAR CORREO ELECTRÓNICO PRIMERO (Con la contraseña limpia)
                string asunto = "Bienvenido a SSMI - Credenciales de Acceso";
                string mensaje = $@"
<h3>¡Hola, {nombre}!</h3>
<p>Has sido registrado como conductor en el Sistema de Seguimiento de Movilidad Integrada (SSMI).</p>
<p>Usa las siguientes credenciales temporales para ingresar al sistema:</p>
<ul>
    <li><strong>Usuario:</strong> {email}</li>
    <li><strong>Contraseña Temporal:</strong> {contrasenaTemporal}</li>
</ul>
<p><em>Al iniciar sesión por primera vez deberás completar tus datos personales.</em></p>";

                // Instanciamos tu clase e invocamos el método con tus parámetros específicos
                Correo correoServicio = new Correo();
                await correoServicio.EnviarCorreoSMTP(nombre, email, asunto, mensaje);

                // 3. ENCRIPTAR LA CONTRASEÑA PARA LA BASE DE DATOS
                Contrasena ctr = new Contrasena();
                string contrasenaEncriptada = ctr.EncriptarContrasena(contrasenaTemporal);

                // 4. Registrar en la Base de Datos la versión ENCRIPTADA
                bool registrado = _consultaConductor.PreRegistrarConductor(nombre, email, contrasenaEncriptada, cadenaConexion);

                if (registrado)
                {
                    TempData["Exito"] = $"El conductor {nombre} fue invitado con éxito. Se envió la contraseña temporal a su correo.";
                    return RedirectToAction(nameof(ConductoresIndex));
                }
                else
                {
                    List<Conductor> listaConductores = _consultaConductor.ObtenerConductoresAdmin(cadenaConexion);
                    ViewBag.Error = "No se pudo completar el pre-registro en la base de datos.";

                    // CORRECCIÓN: Retorna tu vista real con su lista obligatoria
                    return View("ConductoresEdit", listaConductores);
                }
            }
            catch (Exception ex)
            {
                var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;
                List<Conductor> listaConductores = _consultaConductor.ObtenerConductoresAdmin(cadenaConexion);

                ViewBag.Error = "Ocurrió un error inesperado: " + ex.Message;

                // CORRECCIÓN: Retorna tu vista real con su lista obligatoria
                return View("ConductoresEdit", listaConductores);
            }
        }

        // GET: Administrador/Conductores/Edit/5
        public ActionResult ConductoresEdit(int id)
        {
            // OBTENEMOS LOS DATOS AQUÍ TAMBIÉN para que la tabla nunca se quede en blanco
            var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;
            List<Conductor> listaConductores = _consultaConductor.ObtenerConductoresAdmin(cadenaConexion);

            // Pasamos la lista para que el @if (Model != null && Model.Count > 0) de tu vista funcione siempre
            return View("ConductoresEdit", listaConductores);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductoresDelete(string id)
        {
            try
            {
                var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;

                // Si el ID viene vacío, recargamos con error
                if (string.IsNullOrEmpty(id))
                {
                    TempData["MensajeError"] = "El identificador del conductor no es válido o llegó vacío.";
                    return RedirectToAction(nameof(ConductoresIndex));
                }

                // EJECUTAMOS LA INACTIVACIÓN
                // Pasamos el ID directamente. Nota: Si en tu ConsultaConductor.cs el SQL 
                // requiere un UNIQUEIDENTIFIER, recuerda hacer Guid.Parse(idConductor) allá adentro.
                bool inactivado = _consultaConductor.InactivarConductor(id, cadenaConexion);

                if (inactivado)
                {
                    TempData["Exito"] = "¡El conductor ha sido inactivado correctamente!";
                }
                else
                {
                    TempData["MensajeError"] = $"No se pudo actualizar el estado. El ID '{id}' no se modificó en la BD.";
                }

                return RedirectToAction(nameof(ConductoresIndex));
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error crítico al intentar inactivar: " + ex.Message;
                return RedirectToAction(nameof(ConductoresIndex));
            }
        }
        // POST: Administrador/ConductoresActivar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductoresActivar(string id)
        {
            try
            {
                var cadenaConexion = _configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;

                if (string.IsNullOrEmpty(id))
                {
                    TempData["MensajeError"] = "El identificador del conductor no es válido.";
                    return RedirectToAction(nameof(ConductoresIndex));
                }

                // Ejecutamos la activación utilizando el nuevo método
                bool activado = _consultaConductor.ActivarConductor(id, cadenaConexion);

                if (activado)
                {
                    TempData["Exito"] = "¡El conductor ha sido reactivado con éxito!";
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo reactivar el conductor en la base de datos.";
                }

                return RedirectToAction(nameof(ConductoresIndex));
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error crítico al intentar activar: " + ex.Message;
                return RedirectToAction(nameof(ConductoresIndex));
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

        private string GenerarContrasenaAleatoria(int longitud)
        {
            const string caracteres = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            StringBuilder resultado = new StringBuilder();
            Random rnd = new Random();

            while (0 < longitud--)
            {
                resultado.Append(caracteres[rnd.Next(caracteres.Length)]);
            }

            return resultado.ToString();
        }

    }
}
