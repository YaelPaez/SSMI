using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;

namespace SSMI.Funciones
{
    public class Correo
    {
        public async Task EnviarCorreoSMTP(string destinatario,string correoDestino, string Titulo, string Cuerpo)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MAUII APP", "paezyael5im10@gmail.com"));
            message.To.Add(new MailboxAddress(destinatario, correoDestino));
            message.Subject = Titulo;

            message.Body = new TextPart("plain")
            {
                Text = Cuerpo
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("paezyael5im10@gmail.com", "skufhwthuydeawzs"); // no tu contraseña normal
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
