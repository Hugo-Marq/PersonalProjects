using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http.HttpResults;
using QuickRoomSolutions.Models;

namespace QuickRoomSolutions.Notificacoes;
public static class Notificacoes
{
    public static void EnviarLembreteConfirmacao(string clienteEmail, string assunto, string mensagem)
    {
        string smtpServerAddress = "smtp.gmail.com";
        int smtpPort = 587;
        string username = "teste.ams.smtp@gmail.com";
        string password = "exhg onov zfkr uwot";
        string fromEmail = "NoReply@example.com";
        string noReplyName = "No-Reply";
        //string toEmail = cliente.Email;

        using SmtpClient client = new SmtpClient(smtpServerAddress, smtpPort);
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(username, password);

        MailMessage message = new MailMessage(fromEmail, clienteEmail, assunto, mensagem);
        MailAddress noReply = new MailAddress(fromEmail,noReplyName);
        message.From = noReply;
        message.IsBodyHtml = true;
        ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
        client.Send(message);
    }
}
