using Microsoft.Data.SqlClient;
using System.Data;

namespace SSMI.Data
{
    public class ConsultasRutasAdm
    {
        /// <summary>
        /// Obtiene todas las rutas y sus variantes asociadas a un administrador específico.
        /// Ejecuta el stored procedure spConsultarRutasYVariantesAdm.
        /// </summary>
        /// <param name="idAdministrador">ID único del administrador</param>
        /// <param name="cadenaConexion">Cadena de conexión a la base de datos</param>
        /// <returns>Tupla con dos listas: Rutas y RutaVariantes</returns>
        public (List<Ruta> Rutas, List<RutaVariante> Variantes) ConsultarRutasYVariantesAdm(Guid idAdministrador, string cadenaConexion)
        {
            var rutas = new List<Ruta>();
            var variantes = new List<RutaVariante>();

            using (SqlConnection con = new SqlConnection(cadenaConexion))
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
                                com.CommandText = "spConsultarRutasYVariantesAdm";

                                com.Parameters.AddWithValue("@IdAdminisrador", idAdministrador);

                                using (SqlDataReader reader = com.ExecuteReader(CommandBehavior.SequentialAccess))
                                {
                                    // Primera lectura: Rutas
                                    while (reader.Read())
                                    {
                                        var ruta = new Ruta
                                        {
                                            IdRuta = reader["IdRuta"] != DBNull.Value ? (Guid)reader["IdRuta"] : Guid.Empty,
                                            Nombre = reader["Nombre"]?.ToString(),
                                            Descripcion = reader["Descripcion"]?.ToString(),
                                            Administrador = reader["Administrador"] != DBNull.Value ? (Guid)reader["Administrador"] : Guid.Empty,
                                            FechaCreacion = reader["FechaCreacion"] != DBNull.Value ? (DateTime)reader["FechaCreacion"] : DateTime.MinValue,
                                            Activa = reader["Activa"] != DBNull.Value && (bool)reader["Activa"]
                                        };

                                        rutas.Add(ruta);
                                    }

                                    // Avanzar al siguiente resultado (Variantes)
                                    if (reader.NextResult())
                                    {
                                        while (reader.Read())
                                        {
                                            var variante = new RutaVariante
                                            {
                                                IdRutaVariante = reader["IdRutaVariante"] != DBNull.Value ? (int)reader["IdRutaVariante"] : 0,
                                                IdRuta = reader["IdRuta"] != DBNull.Value ? (Guid)reader["IdRuta"] : Guid.Empty,
                                                Nombre = reader["NombreVariante"]?.ToString(),
                                                Nombre_Variante = reader["NombreVariante"]?.ToString(), // Campo alternativo si existe
                                                Sentido = reader["Sentido"]?.ToString(),
                                                Activa = reader["Activa"] != DBNull.Value && (bool)reader["Activa"]
                                            };

                                            variantes.Add(variante);
                                        }
                                    }

                                    trans.Commit();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            Console.WriteLine("ERROR en ConsultarRutasYVariantesAdm: " + ex.Message);
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

            return (rutas, variantes);
        }
    }

    /// <summary>
    /// Modelo para representar una Ruta
    /// </summary>
    public class Ruta
    {
        public Guid IdRuta { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public Guid Administrador { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activa { get; set; }
    }

    /// <summary>
    /// Modelo para representar una Variante de Ruta
    /// </summary>
    public class RutaVariante
    {
        public int IdRutaVariante { get; set; }
        public Guid IdRuta { get; set; }
        public string? Nombre { get; set; }
        public string? Nombre_Variante { get; set; } // Campo alternativo
        public string? Sentido { get; set; }
        public bool Activa { get; set; }
    }
}
