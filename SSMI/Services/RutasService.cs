using SSMI.Data;
using SSMI.Models;

namespace SSMI.Services;

public sealed class RutasService : IRutasService
{
    private readonly ConsultasParadas _consultasParadas;
    private readonly string _connectionString;

    public RutasService(IConfiguration configuration, ConsultasParadas consultasParadas)
    {
        _consultasParadas = consultasParadas;
        _connectionString = configuration.GetConnectionString("StringCONSQLocal") ?? string.Empty;
    }

    public List<Parada> ObtenerParadasCercanas(UbicacionRequest request)
    {
        return _consultasParadas.ConsultarParadas(
            _connectionString,
            request.Lat,
            request.Lon);
    }

    public List<Parada> ObtenerTodasLasParadas()
    {
        return _consultasParadas.ConsultarTodasParadas(_connectionString);
    }

    
}
