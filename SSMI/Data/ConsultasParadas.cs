using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SSMI.Models;
using System.Data;

namespace SSMI.Data
{
    public class ConsultasParadas
    {
        public List<Parada> ConsultarParadas(string cdnConexion, decimal lat, decimal lon)
        {
            List<Parada> paradas = new List<Parada>();

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
                                com.CommandText = "spConsultarTodasLasParadasRespectoPosicion";

                                com.Parameters.AddWithValue("@Lat", lat);
                                com.Parameters.AddWithValue("@Lon", lon);

                                using (SqlDataReader reader = com.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        Parada parada = new Parada
                                        {
                                            IdParada = reader["IdParada"].ToString(),
                                            Lat = Convert.ToDecimal(reader["Lat"]),
                                            Lon = Convert.ToDecimal(reader["Lon"]),
                                            Cercania = Convert.ToDecimal(reader["DistanciaMetros"])
                                        };

                                        paradas.Add(parada);
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
                return paradas;
            }
        }

        public List<Parada> ConsultarTodasParadas(string cdnConexion)
        {
            List<Parada> paradas = new List<Parada>();

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
                                com.CommandText = "spConsultarTodasLasParadas";

                                using (SqlDataReader reader = com.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        Parada parada = new Parada
                                        {
                                            IdParada = reader["IdParada"].ToString(),
                                            Lat = Convert.ToDecimal(reader["Lat"]),
                                            Lon = Convert.ToDecimal(reader["Lon"])
                                        };

                                        paradas.Add(parada);
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
                return paradas;
            }
        }



    }
}
