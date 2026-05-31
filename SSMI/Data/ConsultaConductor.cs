using System.Data;
using Microsoft.Data.SqlClient;
using SSMI.Models;
using SSMI.Models.ViewModels;

namespace SSMI.Data
{
    public class ConsultaConductor
    {
        // METODO 1: Pre-registrar al conductor (Fase 1: Alta del Admin)
        public bool PreRegistrarConductor(string nombre, string correo, string contrasenaTemporal, string cadenaCon)
        {
            bool exito = false;

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand com = new SqlCommand())
                            {
                                com.Connection = con;
                                com.Transaction = trans;
                                com.CommandType = CommandType.StoredProcedure;
                                com.CommandText = "spPreRegistrarConductor";

                                // ══ ALINEACIÓN DE PARÁMETROS C# -> SQL ══
                                com.Parameters.AddWithValue("@Nombre", nombre ?? (object)DBNull.Value);

                                // CORRECCIÓN 1: Cambiamos "@Correo" por "@Email"
                                com.Parameters.AddWithValue("@Email", correo ?? (object)DBNull.Value);

                                // CORRECCIÓN 2: Cambiamos "@Contrasena" por "@ContrasenaTemporal"
                                com.Parameters.AddWithValue("@ContrasenaTemporal", contrasenaTemporal ?? (object)DBNull.Value);

                                com.ExecuteNonQuery();
                            }
                            trans.Commit();
                            exito = true;
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            Console.WriteLine("ERROR EN TRANSACCION CONDUCTOR: " + ex.Message);
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR AL CONECTAR PARA PRE-REGISTRO: " + ex.Message);
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            return exito;
        }

        // METODO 2: Obtener la lista de conductores para la tabla del Administrador
        public List<Conductor> ObtenerConductoresAdmin(string cadenaCon)
        {
            List<Conductor> lista = new List<Conductor>();

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "spObtenerConductoresAdmin";

                        using (SqlDataReader dr = com.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Conductor cond = new Conductor
                                {
                                    Id = dr["IdUsuario"].ToString(),
                                    Nombre = dr["Nombre"].ToString(),
                                    // Controlamos que los campos que nacen NULL en la Fase 1 no rompan el programa
                                    ApellidoPaterno = dr.IsDBNull(dr.GetOrdinal("ApellidoPaterno")) ? "" : dr["ApellidoPaterno"].ToString(),
                                    ApellidoMaterno = dr.IsDBNull(dr.GetOrdinal("ApellidoMaterno")) ? "" : dr["ApellidoMaterno"].ToString(),
                                    Empresa = dr.IsDBNull(dr.GetOrdinal("Empresa")) ? "" : dr["Empresa"].ToString(),
                                    Email = dr["Correo"].ToString(),
                                    Estado = dr["Estado"].ToString()
                                };

                                lista.Add(cond);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR AL OBTENER CONDUCTORES: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            return lista;
        }

        public bool ActualizarPerfilConductor(string correo, string apellidoPaterno, string apellidoMaterno, string empresa, string nuevaContrasenaHash, string cadenaCon)
        {
            bool exito = false;

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    // Usamos una Transacción para asegurar que se actualicen ambas tablas o ninguna
                    using (SqlTransaction trans = con.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand com = new SqlCommand())
                            {
                                com.Connection = con;
                                com.Transaction = trans;
                                com.CommandType = CommandType.Text;

                                // 1. ACTUALIZAR CONTRASEÑA EN tbUsuarios
                                com.CommandText = @"
                            UPDATE tbUsuarios
                            SET Contrasena = @Contrasena
                            WHERE Correo = @Correo";

                                com.Parameters.AddWithValue("@Correo", correo);
                                com.Parameters.AddWithValue("@Contrasena", nuevaContrasenaHash);

                                com.ExecuteNonQuery();

                                // Limpiamos parámetros para la siguiente consulta
                                com.Parameters.Clear();

                                // 2. ACTUALIZAR DATOS Y ESTADO EN tbConductores
                                // Hacemos un INNER JOIN con tbUsuarios para encontrar al conductor mediante su Correo
                                com.CommandText = @"
                            UPDATE C
                            SET C.ApellidoPaterno = @ApellidoPaterno,
                                C.ApellidoMaterno = @ApellidoMaterno,
                                C.Empresa = @Empresa,
                                C.Estado = 'Activo'
                            FROM tbConductores C
                            INNER JOIN tbUsuarios U ON C.IdUsuario = U.IdUsuario
                            WHERE U.Correo = @Correo";

                                com.Parameters.AddWithValue("@Correo", correo);
                                com.Parameters.AddWithValue("@ApellidoPaterno", apellidoPaterno ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@ApellidoMaterno", apellidoMaterno ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@Empresa", empresa ?? (object)DBNull.Value);

                                int filasAfectadas = com.ExecuteNonQuery();

                                // Si se modificó el registro del conductor, la operación fue un éxito
                                exito = filasAfectadas > 0;
                            }

                            // Si todo salió bien, guardamos los cambios definitivamente
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            // Si algo falla, deshacemos cualquier cambio para no dejar la BD inconsistente
                            trans.Rollback();
                            Console.WriteLine("ERROR AL ACTUALIZAR PERFIL DE CONDUCTOR: " + ex.Message);
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR DE CONEXIÓN AL ACTUALIZAR CONDUCTOR: " + ex.Message);
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }

            return exito;
        }

        public bool InactivarConductor(string idConductor, string cadenaConexion)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    // CORRECCIÓN: Cambiamos 'Usuarios' por 'tbConductores' que es tu tabla real
                    string query = "UPDATE tbConductores SET Estado = 'Inactivo' WHERE IdUsuario = @Id";

                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        // Pasamos el GUID parseado correctamente
                        comando.Parameters.AddWithValue("@Id", Guid.Parse(idConductor));

                        conexion.Open();
                        int filasAfectadas = comando.ExecuteNonQuery();

                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool ActivarConductor(string idConductor, string cadenaConexion)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    // Cambiamos el Estado a 'Activo'
                    string query = "UPDATE tbConductores SET Estado = 'Activo' WHERE IdUsuario = @Id";

                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@Id", Guid.Parse(idConductor));

                        conexion.Open();
                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool ExisteCorreoConductor(string email, string cadenaConexion)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    // CORRECCIÓN: La columna real en tbUsuarios es [Correo]
                    string query = "SELECT COUNT(1) FROM tbUsuarios WHERE LOWER(LTRIM(RTRIM(Correo))) = LOWER(@Email)";

                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        comando.Parameters.AddWithValue("@Email", email.Trim());

                        conexion.Open();
                        int existe = Convert.ToInt32(comando.ExecuteScalar());

                        return existe > 0; // Si encuentra el registro en tbUsuarios, devuelve true
                    }
                }
            }
            catch (Exception)
            {
                // En caso de cualquier error de conexión o consulta, por seguridad devolvemos false
                return false;
            }
        }
        public CompletarPerfilViewModel? ObtenerPerfilPorEmail(string email, string connectionString)
        {
            CompletarPerfilViewModel? conductor = null;

            // Consulta utilizando INNER JOIN para traer los apellidos y empresa de tbConductores
            // y el correo de tbUsuarios usando el IdUsuario como llave de unión.
            string query = @"
                SELECT 
                    u.Correo, 
                    c.ApellidoPaterno, 
                    c.ApellidoMaterno, 
                    c.Empresa 
                FROM dbo.tbUsuarios u
                INNER JOIN dbo.tbConductores c ON u.IdUsuario = c.IdUsuario
                WHERE u.Correo = @Correo";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Correo", email);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            conductor = new CompletarPerfilViewModel
                            {
                                Email = reader["Correo"].ToString(),
                                ApellidoPaterno = reader["ApellidoPaterno"] != DBNull.Value ? reader["ApellidoPaterno"].ToString() : "",
                                ApellidoMaterno = reader["ApellidoMaterno"] != DBNull.Value ? reader["ApellidoMaterno"].ToString() : "",
                                Empresa = reader["Empresa"] != DBNull.Value ? reader["Empresa"].ToString() : ""
                            };
                        }
                    }
                }
            }

            return conductor;
        }
    }
}