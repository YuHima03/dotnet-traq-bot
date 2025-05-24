using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    /// <param name="traqOptions"></param>
    /// <param name="logger"></param>
    /// <param name="baseLogger"></param>
    public abstract class TraqWsBot(
        IOptions<TraqApiClientOptions> traqOptions,
        ILogger<TraqWsBot>? logger,
        ILogger<TraqBot> baseLogger
        ) : TraqBot(baseLogger)
    {
        const int WsBufferSize = 1 << 16;

        ClientWebSocket? _ws = null;
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
            var ws = (_ws ??= await CreateAndStartClientWebSocketAsync(traqOptions.Value, ct));

            var result = await ws.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                logger?.LogWarning("Received a close message: {}", result.CloseStatusDescription);
                using (ws)
                {
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, ct);
                }
                _ws = null;
                return false;
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                logger?.LogError("Received a binary message.");
                return false;
            }
            else if (!result.EndOfMessage)
            {
                logger?.LogError("Received too long message: {} bytes.", result.Count);
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
            _ws = await CreateAndStartClientWebSocketAsync(traqOptions.Value, ct);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await (_ws?.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken) ?? Task.CompletedTask);
        }

        /// <summary>
        /// Ensures that a WebSocket client is created and started.
        /// </summary>
        async ValueTask<ClientWebSocket> CreateAndStartClientWebSocketAsync(TraqApiClientOptions options, CancellationToken ct)
        {
            if (options.BaseAddressUri is null)
            {
                throw new ArgumentException($"{nameof(TraqApiClientOptions.BaseAddressUri)} must be set before creating a WebSocket client.");
            }

            var uri = new UriBuilder(options.BaseAddressUri)
            {
                Path = Path.Combine(options.BaseAddressUri.AbsolutePath, "bots/ws"),
                Scheme = (options.BaseAddressUri.Scheme == Uri.UriSchemeHttps) ? Uri.UriSchemeWss : Uri.UriSchemeWs
            }.Uri;

            var ws = CreateClientWebSocket(options);
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    await ws.ConnectAsync(uri, ct);
                    logger?.LogInformation("Connected to a WebSocket server: {Uri}", uri);
                    return ws;
                }
                catch (WebSocketException e)
                {
                    logger?.LogError(e, "Failed to connect to a WebSocket server. Retry after a minute.");
                    await Task.Delay(TimeSpan.FromMinutes(1), ct);
                }
            }
        }

        static ClientWebSocket CreateClientWebSocket(TraqApiClientOptions options)
        {
            ClientWebSocket ws = new();

            if (!string.IsNullOrEmpty(options.BearerAuthToken))
            {
                ws.Options.SetRequestHeader("Authorization", $"Bearer {options.BearerAuthToken}");
            }
            else if (!string.IsNullOrEmpty(options.CookieAuthToken) && options.BaseAddressUri is not null)
            {
                CookieContainer cc = ws.Options.Cookies ?? new();
                cc.Add(new Cookie
                {
                    Name = "r_session",
                    Value = options.CookieAuthToken,
                    Domain = options.BaseAddressUri.Host,
                    Path = "/",
                    HttpOnly = true,
                    Secure = options.BaseAddressUri.Scheme == Uri.UriSchemeHttps
                });
                ws.Options.Cookies = cc;
            }

            return ws;
        }
    }
}
