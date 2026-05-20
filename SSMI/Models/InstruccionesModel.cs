namespace SSMI.Models
{
    public class InstruccionesModel
    {
        public string Tipo { get; set; }
        public string IndicacionTexto { get; set; }
        public int cantidad {  get; set; }
        public string medida { get; set; }
        public decimal PosicionLat { get; set; }
        public decimal PosicionLon { get; set; }

    }
}
