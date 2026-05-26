namespace SSMI.Models
{
    public class RegistroHistoricoModel
    {

        public int Id { get; set; }
        public string NumEconomico { get; set; }
        public string Conductor { get; set; }
        public DateTime Fecha { get; set; }
        public string Hora { get; set; }
        public string Estado { get; set; }

    }
}
