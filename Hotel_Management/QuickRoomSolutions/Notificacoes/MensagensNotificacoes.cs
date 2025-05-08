using QuickRoomSolutions.DTOs;
using QuickRoomSolutions.Models;

namespace MensagensNotificacoes;

public static class MensagensNotificacoes
{
      public static readonly string style = @"
    body {
        font-family: Arial, sans-serif;
        background-color: #f4f4f4;
        margin: 0;
        padding: 0;
    }
    .container {
        max-width: 600px;
        margin: 20px auto;
        background-color: #fff;
        padding: 20px;
        border-radius: 5px;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
    }
    h1 {
        color: #333;
        text-align: center;
    }
    p {
        color: #666;
    }
";



    public static string politicasCancelamento()
    {
        string cancelamento = "Todas as reservas no nosso hotel requerem um cartão de crédito para garantia. Cancelamentos podem ser feitos sem custo adicional até 24 horas antes do check-in.<br>";
        cancelamento += "Após esse prazo, e em caso de cancelamento ou não comparência, será debitado o valor da primeira noite.<br>";
        cancelamento += "No entanto, se o cancelamento for comunicado entre as 24 horas antes do check-in e o momento do check-in, será oferecido um crédito do valor igual ao debitado para garantia da primeira noite.<br>";
        cancelamento +=  "Este crédito será válido por um período de 30 dias e poderá ser utilizado para uma nova reserva em qualquer um dos nossos estabelecimentos.<br>";

        return cancelamento;
    }

    public static string CriarMensagemConfirmacaoReserva(ReservaDTO reserva, Pessoa cliente)
{
    string dataInicioFormatada = reserva.DataInicio.ToString("dd/MM/yyyy");
    string dataFimFormatada = reserva.DataFim.ToString("dd/MM/yyyy");

    TimeSpan duracaoEstadia = reserva.DataFim - reserva.DataInicio;
    int diasEstadia = duracaoEstadia.Days;
    string politicaCancelamento = politicasCancelamento();
    string mensagem = $@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <style>
                {style}
            </style>
            <title>Confirmação de Reserva</title>
        </head>
        <body>
            <div class=""container"">
                 <img src=""https://i.postimg.cc/vHfRKByC/LogoQRS.jpg"" alt=""Logo QuickRoomSolutions"" style=""width: 150px; position: absolute; top: 10px; left: 10px;"" />
                <h1>Confirmação de Reserva</h1>
                <p>Olá {cliente.Nome},</p>
                <p>A sua reserva foi confirmada com sucesso!</p>
                <p>Detalhes da reserva:</p>
                <ul>
                    <li>Data de check-in: {dataInicioFormatada}</li>
                    <li>Data de check-out: {dataFimFormatada}</li>
                    <li>Duração da estadia: {diasEstadia} dias</li>
                </ul>
                <p>{politicaCancelamento}</p>
                <p>Obrigado por escolher os nossos serviços. Estamos ansiosos para recebê-lo!</p>
                <p>Atenciosamente,<br>[QuickRoomSolutions]</p>
            </div>
        </body>
        </html>
    ";

    return mensagem;
}
    public static string CriarMensagemTerminoLimpeza(TicketLimpeza ticketLimpeza , LimpezaEstados limpezaEstado)
    {
        if (limpezaEstado != LimpezaEstados.Finalizada && limpezaEstado != LimpezaEstados.FinalizadaProblemas)
        {
            return null;
        }

        string estado = limpezaEstado == LimpezaEstados.Finalizada ? "concluída com sucesso" : "concluída com problemas";

        string mensagem = $@"<!DOCTYPE html>
                            <html lang=""pt-pt"">
                            <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Email de Término de Limpeza</title>
                            <style>
                                {style}
                            </style>
                            </head>
                            <body>
                            <div class=""container"">
                                <img src=""https://i.postimg.cc/vHfRKByC/LogoQRS.jpg"" alt=""Logo QuickRoomSolutions"" style=""width: 150px; position: absolute; top: 10px; left: 10px;"" />
                                <p>A limpeza do quarto " + ticketLimpeza.QuartoQuartoId + " referente ao ticket nº" + ticketLimpeza.LimpezaId + @" foi concluída com " + estado +@".</p>
                                <p>Atenciosamente,<br>
                                [QuickRoomSolutions]</p>
                            </div>
                            </body>
                            </html>";

        return mensagem;
    }

    public static string CriarMensagemTerminoManutencao(Ticket ticket)
    {
        string mensagem = $@"<!DOCTYPE html>
                            <html lang=""pt-pt"">
                            <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Email de Término de Manutenção</title>
                            <style>
                                {style}
                            </style>
                            </head>
                            <body>
                            <div class=""container"">
                                <img src=""https://i.postimg.cc/vHfRKByC/LogoQRS.jpg"" alt=""Logo QuickRoomSolutions"" style=""width: 150px; position: absolute; top: 10px; left: 10px;"" />
                                <p>A manutenção do quarto " + ticket.QuartoQuartoId + " referente ao ticket nº" + ticket.TicketId + @" foi finalizada.</p>
                                <p>Por favor, verifique o estado do quarto e confirme a conclusão.</p>
                                <p>Atenciosamente,<br>
                                [QuickRoomSolutions]</p>
                            </div>
                            </body>
                            </html>";

        return mensagem;
    }

}