using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using Traq.Bot.Helpers;

namespace Traq.Bot.WebSocket
{
    /// <summary>
    /// A base class for implementing a traQ BOT service that receives messages with WebSocket.
    /// </summary>
    /// <param name="traq"></param>
    /// <param name="provider"></param>
    public abstract class TraqWsBot(ITraqApiClient traq, IServiceProvider provider) : TraqBot(provider)
    {
        const int WsBufferSize = 1 << 16;

        readonly ILogger<TraqWsBot> _logger = provider.GetRequiredService<ILogger<TraqWsBot>>();
        readonly ITraqApiClient _traq = traq;

        readonly ClientWebSocket _ws = CreateClientWebSocket(traq);
        readonly byte[] _wsBuffer = ArrayPool<byte>.Shared.Rent(WsBufferSize);

        (string? RequestId, string EventName, JsonElement body) _current;

        public override void Dispose()
        {
            if (_wsBuffer is not null)
            {
                ArrayPool<byte>.Shared.Return(_wsBuffer);
            }

            base.Dispose();
            _ws?.Dispose();

            GC.SuppressFinalize(this);
        }

        protected sealed override async ValueTask<(string? RequestId, string EventName, JsonElement Body)> WaitForNextEventAsync(CancellationToken ct)
        {
            var ts500ms = TimeSpan.FromMilliseconds(500);

            while (!ct.IsCancellationRequested)
            {
                var received = HandleMessageAsync(ct);
                await Task.WhenAll(received, Task.Delay(ts500ms, ct));

                if (received.Result)
                {
                    return _current;
                }
            }
            throw new OperationCanceledException(ct);
        }

        async Task<bool> HandleMessageAsync(CancellationToken ct)
        {
            byte[] buffer = _wsBuffer;
            var logger = _logger;
            var ws = _ws;

            var result = await ws.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                logger?.LogWarning("Received a close message: {}", result.CloseStatusDescription);
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, ct);
                await StartWebSocketAsync(ct);
                return false;
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                logger?.LogError("Received a binary message.");
                await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Binary message is not supported.", ct);
                await StartWebSocketAsync(ct);
                return false;
            }
            else if (!result.EndOfMessage)
            {
                logger?.LogError("Received too long message: {} bytes.", result.Count);
                await ws.CloseAsync(WebSocketCloseStatus.MessageTooBig, null, ct);
                await StartWebSocketAsync(ct);
                return false;
            }
            else if (result.Count == 0)
            {
                return false;
            }

            Utf8JsonReader reader = new(buffer.AsSpan()[..result.Count]);
            var doc = JsonDocument.ParseValue(ref reader);

            var jsonRoot = doc.RootElement;
            var eventName = jsonRoot.GetProperty("type").GetString().MustNotNull();
            var body = jsonRoot.GetProperty("body");
            var requestId = (eventName == TraqBotEvents.Error) ? null : jsonRoot.GetProperty("reqId").GetString();

            _current = (requestId, eventName, body);
            return true;
        }

        protected override async ValueTask InitializeAsync(CancellationToken ct)
        {
            await base.InitializeAsync(ct);
            await StartWebSocketAsync(ct);
        }

        async ValueTask StartWebSocketAsync(CancellationToken ct)
        {
            var logger = _logger;
            var ws = _ws;

            UriBuilder ub = new(_traq.Options.BaseAddress);
            ub.Path = Path.Combine(ub.Path, "bots/ws");
            ub.Scheme = (ub.Scheme == Uri.UriSchemeHttps) ? Uri.UriSchemeWss : Uri.UriSchemeWs;

            var uri = ub.Uri;

            try
            {
                await ws.ConnectAsync(uri, ct);
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Failed to connect to a WebSocket server.");
                throw;
            }
            logger?.LogInformation("Successfully connected to a WebSocket server. -> {}", uri);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            if (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
            }
        }

        static ClientWebSocket CreateClientWebSocket(ITraqApiClient traqApiClient)
        {
            ClientWebSocket ws = new();

            if (!string.IsNullOrEmpty(traqApiClient.Options.BearerAuthToken))
            {
                ws.Options.SetRequestHeader("Authorization", $"Bearer {traqApiClient.Options.BearerAuthToken}");
            }
            else if (traqApiClient.ClientHandler is not null)
            {
                UriBuilder baseAddressBuilder = new(traqApiClient.Options.BaseAddress)
                {
                    Fragment = "",
                    Path = "",
                    Query = ""
                };

                var cookie = traqApiClient.ClientHandler.CookieContainer.GetCookies(baseAddressBuilder.Uri).Where(c => c.Name == "r_session");
                if (cookie.Count() == 1)
                {
                    CookieContainer cc = ws.Options.Cookies ?? new();
                    cc.Add(cookie.First());
                    ws.Options.Cookies = cc;
                }
            }

            return ws;
        }
    }
}
