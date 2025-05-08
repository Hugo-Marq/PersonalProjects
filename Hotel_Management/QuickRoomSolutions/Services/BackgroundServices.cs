using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickRoomSolutions.Models;
using QuickRoomSolutions.Respositories.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BackgroundServices : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundServices> _logger;
    private Timer _timer;

    public BackgroundServices(IServiceScopeFactory scopeFactory, ILogger<BackgroundServices> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification Service running.");

        // Set the timer to run daily at 8 AM
        var now = DateTime.UtcNow;
        var today8AM = DateTime.Today.AddHours(8);
        var nextRun = now > today8AM ? today8AM.AddDays(1) : today8AM;
        var firstRunDelay = (nextRun - now).TotalMilliseconds;


        try
        {
            _timer = new Timer(ExecuteTask, null, (int)firstRunDelay, (int)TimeSpan.FromDays(1).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing timer.");
        }

        return Task.CompletedTask;
    }

    private void ExecuteTask(object state)
    {
        _logger.LogInformation("Executing daily notification task.");
        Task.Run(async () =>  // Use Task.Run to handle the async calls
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketLimpezaRepository<TicketLimpeza>>();

                // Await the asynchronous method to ensure it completes before continuing
                var tickets = await ticketRepository.GetByPriorityOrder();
                if (tickets.Any())
                {
                    var messageBody = $"Rooms to be cleaned today:\n{string.Join("\n", tickets.Select(t => $"Quarto {t.QuartoQuartoId}, Prioridade: {t.LimpezaPrioridade}"))}";
                    QuickRoomSolutions.Notificacoes.Notificacoes.EnviarLembreteConfirmacao("devtest_pikos@hotmail.com", "Lista diária de quartos a limpar", messageBody);
                }
            }
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                // Log any exceptions that were thrown during the execution of the task
                _logger.LogError(task.Exception, "Error executing async task in NotificationService.");
            }
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
