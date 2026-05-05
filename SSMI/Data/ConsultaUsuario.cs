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

                                com.Parameters.AddWithValue("@Rol", usuario.Rol ?? (object)DBNull.Value);
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

        public Usuario ConsultarUsuario(string correo, string cadenaCon)
        {
            Usuario usuario = new Usuario();

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
                        com.Parameters.AddWithValue("@Correo", correo ?? (object)DBNull.Value);

                        using (SqlDataReader dr = com.ExecuteReader())
                        {
                            if (dr.Read())
                            {
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
