using SSMI.Data;

namespace SSMI.Models.ViewModels
{
    public class RutaViewModelAdm
    {
        public List<Ruta> Rutas { get; set; } = new List<Ruta>();
        public List<RutaVariante> Variantes { get; set; } = new List<RutaVariante>();
    }
}
