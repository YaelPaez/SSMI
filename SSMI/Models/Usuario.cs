namespace SSMI.Models
{
    public class Usuario
    {
        public string ID { get; set; }
        public string Rol { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Contrasena { get; set; }
        public string Correo { get; set; }
        public string Numtelefono { get; set; }
        public string Genero { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string Discapacidad { get; set; }

    }
}
