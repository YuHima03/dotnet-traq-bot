using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Traq.Bot.Helpers;
using Traq.Bot.Models;

namespace Traq.Bot
{
    public abstract class TraqBot(IServiceProvider provider) : BackgroundService
    {
        readonly ILogger<TraqBot> _logger = provider.GetRequiredService<ILogger<TraqBot>>();

        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await InitializeAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var (reqId, evName, body) = await WaitForNextEventAsync(stoppingToken);
                    await HandleEventAsync(reqId, evName, body, stoppingToken);
                }
            }
            catch (OperationCanceledException ex) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "The operation is cancelled because cancellation is requested.");
            }
        }

        ValueTask HandleEventAsync(string? requestId, string eventName, JsonElement body, CancellationToken ct)
        {
            switch (eventName)
            {
                case "JOIN":
                    return OnJoinedAsync(body.Deserialize<JoinOrLeftEventArgs>().MustNotNull(), ct);
                case "LEFT":
                    return OnLeftAsync(body.Deserialize<JoinOrLeftEventArgs>().MustNotNull(), ct);
                case "PING":
                    return OnPingAsync(body.Deserialize<PingEventArgs>().MustNotNull(), ct);
                default:
                    _logger.LogWarning("Unknown event name: {}", eventName);
                    return ValueTask.CompletedTask;
            }
        }

        protected virtual ValueTask InitializeAsync(CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnJoinedAsync(JoinOrLeftEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnLeftAsync(JoinOrLeftEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnPingAsync(PingEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        public sealed override Task StartAsync(CancellationToken cancellationToken) => base.StartAsync(cancellationToken);

        protected abstract ValueTask<(string? RequestId, string EventName, JsonElement Body)> WaitForNextEventAsync(CancellationToken ct);
    }
}
