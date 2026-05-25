using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SSMI.Models;
using System.Data;
namespace SSMI.Data
{
    public class ConsultaUsuario
    {
        public void RegistrarUsuario(Usuario usuario, string cadenaCon)
        {
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
                                com.CommandText = "spRegistrarPasajero";

                                com.Parameters.AddWithValue("@Nombres", usuario.Nombres ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@Apellidos", usuario.Apellidos ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@Contrasena", usuario.Contrasena ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@Correo", usuario.Correo ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@Numtelefono", usuario.Numtelefono ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@Genero", usuario.Genero ?? (object)DBNull.Value);
                                com.Parameters.AddWithValue("@FechaNacimiento", usuario.FechaNacimiento);
                                com.Parameters.AddWithValue("@Discapacidad", usuario.Discapacidad ?? (object)DBNull.Value);

                                com.ExecuteNonQuery();
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            Console.WriteLine("ERROR: " + ex.Message);
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public Usuario? ConsultarUsuario(string correo, string cadenaCon)
        {
            correo = correo?.Trim();
            if (string.IsNullOrWhiteSpace(correo))
            {
                return null;
            }

            Usuario? usuario = null;

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "spConsultarUsuario";
                        com.Parameters.AddWithValue("@Correo", correo);

                        using (SqlDataReader dr = com.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                usuario = new Usuario();
                                usuario.ID = dr["IdUsuario"].ToString();
                                usuario.Rol = dr["Rol"].ToString();
                                usuario.Contrasena = dr.IsDBNull(dr.GetOrdinal("Contrasena")) ? null : dr["Contrasena"].ToString();
                                usuario.Correo = dr["Correo"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }
                return usuario;
            }
        }

        public Usuario? ConsultarPasajeroPerfil(string correo, string cadenaCon)
        {
            correo = correo?.Trim();
            if (string.IsNullOrWhiteSpace(correo))
            {
                return null;
            }

            Usuario? usuario = null;

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "spConsultarPasajero";
                        com.Parameters.AddWithValue("@Correo", correo);

                        using (SqlDataReader dr = com.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                usuario = new Usuario();

                                if (HasColumn(dr, "ID")) usuario.ID = dr["ID"].ToString();
                                if (HasColumn(dr, "IdUsuario") && string.IsNullOrWhiteSpace(usuario.ID)) usuario.ID = dr["IdUsuario"].ToString();

                                usuario.Correo = HasColumn(dr, "Correo") ? dr["Correo"].ToString() : correo;
                                usuario.Nombres = HasColumn(dr, "Nombres") ? dr["Nombres"].ToString() : null;
                                usuario.Apellidos = HasColumn(dr, "Apellidos") ? dr["Apellidos"].ToString() : null;
                                usuario.Numtelefono = HasColumn(dr, "Numtelefono") ? dr["Numtelefono"].ToString() : null;
                                usuario.Genero = HasColumn(dr, "Genero") ? dr["Genero"].ToString() : null;
                                usuario.Discapacidad = HasColumn(dr, "Discapacidad") ? dr["Discapacidad"].ToString() : null;

                                if (TryReadDateOnly(dr, "FechaNacimiento", out var fechaNacimiento) ||
                                    TryReadDateOnly(dr, "FchaNacimiento", out fechaNacimiento))
                                {
                                    usuario.FechaNacimiento = fechaNacimiento;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }

                return usuario;
            }
        }

        private static bool HasColumn(IDataRecord reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadDateOnly(IDataRecord reader, string columnName, out DateOnly date)
        {
            date = default;

            if (!HasColumn(reader, columnName))
            {
                return false;
            }

            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return false;
            }

            var value = reader.GetValue(ordinal);

            if (value is DateOnly d)
            {
                date = d;
                return true;
            }

            if (value is DateTime dt)
            {
                date = DateOnly.FromDateTime(dt);
                return true;
            }

            var s = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            // A veces SQL regresa datetime como string con hora, así que intentamos DateTime primero.
            if (DateTime.TryParse(s, out var dt2))
            {
                date = DateOnly.FromDateTime(dt2);
                return true;
            }

            if (DateOnly.TryParse(s, out var d2))
            {
                date = d2;
                return true;
            }

            return false;
        }

        public List<Usuario> ConsultarUsuarios(string cadenaCon)
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "spConsultarUsuarios";

                        using (SqlDataReader dr = com.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Usuario usuario = new Usuario();

                                usuario.ID = dr["ID"].ToString();
                                usuario.Rol = dr["Rol"].ToString();
                                usuario.Nombres = dr["Nombres"].ToString();
                                usuario.Apellidos = dr["Apellidos"].ToString();
                                usuario.Contrasena = dr["Contrasena"].ToString();
                                usuario.Correo = dr["Correo"].ToString();
                                usuario.Numtelefono = dr["Numtelefono"].ToString();
                                usuario.Genero = dr["Genero"].ToString();
                                usuario.FechaNacimiento = DateOnly.Parse(dr["FechaNacimiento"].ToString());
                                usuario.Discapacidad = dr["Discapacidad"].ToString();

                                usuarios.Add(usuario);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }
                return usuarios;
            }
        }
    }
}
