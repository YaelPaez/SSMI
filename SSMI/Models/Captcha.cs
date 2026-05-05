namespace SSMI.Models
{
    
    public class Captcha
    {
        public string GenerarCaptcha()
        {
            string Captcha = "";

            for (int i = 0; i <= 2; i++)
            {
                // Se inicia el aleatorio para los numeros
                var guid = Guid.NewGuid();
                var justNumbers = new String(guid.ToString()
                    .Where(Char.IsDigit)
                    .ToArray());

                var seed = int.Parse(justNumbers.Substring(0, 4));
                var random = new Random(seed);
                var value = random.Next(0, 9);

                Captcha = Captcha + value.ToString();

                // Se inicia el aleatorio para las letras
                int numero = random.Next(26);
                char letra = (char)(((int)'a') + numero);

                Captcha = Captcha + letra;
            }

            return Captcha;
        }
    }
}
