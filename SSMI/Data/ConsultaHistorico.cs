using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SSMI.Models;
using System.Data;
namespace SSMI.Data
{
    public class ConsultaHistorico
    {
        public List<RegistroHistoricoModel> ObtenerHistoricoFiltrado(string cdnConexion, string conductor, DateTime? inicio, DateTime? fin, string estado)
        {
            List<RegistroHistoricoModel> registros = new List<RegistroHistoricoModel>();

            using (SqlConnection con = new SqlConnection(cdnConexion))
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
                                com.CommandText = "sp_ConsultarHistorico";

                                // Agregamos los parámetros controlando los nulos igual que en la BD
                                com.Parameters.AddWithValue("@Conductor", string.IsNullOrEmpty(conductor) ? (object)DBNull.Value : conductor);
                                com.Parameters.AddWithValue("@FechaInicio", inicio.HasValue ? (object)inicio.Value : DBNull.Value);
                                com.Parameters.AddWithValue("@FechaFin", fin.HasValue ? (object)fin.Value : DBNull.Value);
                                com.Parameters.AddWithValue("@Estado", string.IsNullOrEmpty(estado) ? (object)DBNull.Value : estado);

                                using (SqlDataReader reader = com.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        RegistroHistoricoModel registro = new RegistroHistoricoModel
                                        {
                                            Id = Convert.ToInt32(reader["Id"]),
                                            NumEconomico = reader["NumEconomico"].ToString(),
                                            Conductor = reader["Conductor"].ToString(),
                                            Fecha = Convert.ToDateTime(reader["Fecha"]),
                                            Hora = reader["Hora"].ToString(),
                                            Estado = reader["Estado"].ToString()
                                        };

                                        registros.Add(registro);
                                    }
                                }
                                trans.Commit();
                            }
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

                return registros;
            }
        }
    }
}
