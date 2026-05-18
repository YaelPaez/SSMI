using SSMI.Models;

namespace SSMI.Services;

public interface IRutasService
{
    List<Parada> ObtenerParadasCercanas(UbicacionRequest request);
    List<Parada> ObtenerTodasLasParadas();
}
