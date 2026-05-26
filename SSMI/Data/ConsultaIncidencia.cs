using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SSMI.Models;
using System.Data;

namespace SSMI.Data
{
    public class ConsultaIncidencia
    {
        public void RegistrarIncidencia(IncidenciaModel incidencia, string cadenaCon)
        {
            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "sp_InsertarIncidencia";

                        com.Parameters.AddWithValue("@TipoIncidente", incidencia.TipoIncidente);
                        com.Parameters.AddWithValue("@Descripcion", incidencia.Descripcion);
                        com.Parameters.AddWithValue("@RutaEvidencia",
                            (object?)incidencia.RutaEvidencia ?? DBNull.Value);

                        com.ExecuteNonQuery();
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

        public List<IncidenciaModel> ConsultarIncidencias(string cadenaCon)
        {
            List<IncidenciaModel> incidencias = new List<IncidenciaModel>();

            using (SqlConnection con = new SqlConnection(cadenaCon))
            {
                try
                {
                    con.Open();

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.CommandType = CommandType.StoredProcedure;
                        com.CommandText = "sp_ConsultarIncidencias";

                        using (SqlDataReader dr = com.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                incidencias.Add(new IncidenciaModel
                                {
                                    Id = Convert.ToInt32(dr["Id"]),
                                    TipoIncidente = dr["TipoIncidente"].ToString(),
                                    Descripcion = dr["Descripcion"].ToString(),
                                    RutaEvidencia = dr["RutaEvidencia"].ToString(),
                                    FechaReporte = Convert.ToDateTime(dr["FechaReporte"]),
                                    Estado = dr["Estado"].ToString()
                                });
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
            }

            return incidencias;
        }
    }
}
