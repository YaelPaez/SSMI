using BCrypt.Net;
using System.Text.RegularExpressions;

namespace SSMI.Funciones;
public class Contrasena 
{
    

    public string EncriptarContrasena(string contrasena)
    {
        string r = BCrypt.Net.BCrypt.HashPassword(contrasena);

        return r;
    }

    public bool CompararContrsanas(string contrasena, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(contrasena, hash);
    }

    public string RevisarContrasenas(string contra, string contraRep)
    {
        
        contra ??= string.Empty;
        var caracteres = contra.ToCharArray();

        if (contra != contraRep)
        {
            return "La Contrasenas Deben Coincidir";
        }
        if (caracteres.Length < 8)
        {
            return "La Contrasena Debe Tener Minimo 8 caracteres";
        }
        if (caracteres.Length > 15)
        {
            return "La Contrasena Debe Tener Maximo 15 caracteres";
        }
        if (caracteres.Length == 0 || !Regex.IsMatch(caracteres[0].ToString(), "[A-Z]"))
        {
            return "La contrase�a debe contener al menos una letra may�scula";
        }
        if (!Regex.IsMatch(contra, "[&%$]"))
        {
            return "La contrase�a debe contener al menos un & o % o $";
        }
        return "";
    }
}