using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using Traq.Bot.Helpers;

using WebSocketEventData = (string? RequestId, string EventName, System.Text.Json.JsonElement Body);

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

        WebSocketEventData _current;

        /// <inheritdoc />
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

        /// <summary>
        /// Returns a <see cref="ValueTask{TResult}"/> that completes when the next event is received.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that provides received event data.</returns>
        /// <exception cref="OperationCanceledException"></exception>
        protected sealed override async ValueTask<WebSocketEventData> WaitForNextEventAsync(CancellationToken ct)
        {
            var ts500ms = TimeSpan.FromMilliseconds(500);

            while (!ct.IsCancellationRequested)
            {
                var received = ReceiveAndHandleMessageAsync(ct);
                await Task.WhenAll(received, Task.Delay(ts500ms, ct));

                if (received.Result)
                {
                    return _current;
                }
            }
            return ThrowHelper.ThrowOperationCanceledException<WebSocketEventData>(ct);
        }

        async Task<bool> ReceiveAndHandleMessageAsync(CancellationToken ct)
        {
            byte[] buffer = _wsBuffer;
            var ws = (_ws ??= await CreateAndStartClientWebSocketAsync(traqOptions.Value, ct));

            WebSocketReceiveResult receiveResult;
            try
            {
                receiveResult = await ws.ReceiveAsync(buffer, ct);
            }
            catch (Exception e)
                {
                logger?.LogError(e, "Failed to receive a WebSocket message.");
                Interlocked.Exchange(ref _ws, null)?.Dispose();
                return false;
                }

            if (receiveResult.MessageType is WebSocketMessageType.Close)
            {
                logger?.LogWarning("WebSocket connection was closed: {}", receiveResult.CloseStatusDescription);
                Interlocked.Exchange(ref _ws, null)?.CloseOutputAsync(receiveResult.CloseStatus ?? WebSocketCloseStatus.Empty, receiveResult.CloseStatusDescription, ct);
                return false;
            }
            else if (receiveResult.MessageType is WebSocketMessageType.Binary)
            {
                logger?.LogWarning("Binary message is not supported. The received message is ignored.");
                return false;
            }
            else if (!receiveResult.EndOfMessage)
            {
                logger?.LogWarning("Received too long message: {} bytes. The received message is ignored.", receiveResult.Count);
                return false;
            }
            else if (receiveResult.Count == 0)
            {
                return false;
            }

            _current = GetWebSocketEventData(buffer.AsSpan(0, receiveResult.Count));
            return true;
        }

        /// <inheritdoc />
        protected override async ValueTask InitializeAsync(CancellationToken ct)
        {
            await base.InitializeAsync(ct);
            _ws = await CreateAndStartClientWebSocketAsync(traqOptions.Value, ct);
        }

        /// <inheritdoc />
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            var ws = Interlocked.Exchange(ref _ws, null);
            if (ws is not null && ws.State is WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
            }
        }

        /// <summary>
        /// Ensures that a WebSocket client is created and started.
        /// </summary>
        async ValueTask<ClientWebSocket> CreateAndStartClientWebSocketAsync(TraqApiClientOptions options, CancellationToken ct)
        {
            Guard.IsNotNull(options.BaseAddressUri);

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

        static WebSocketEventData GetWebSocketEventData(ReadOnlySpan<byte> jsonData)
        {
            Utf8JsonReader reader = new(jsonData);
            var doc = JsonDocument.ParseValue(ref reader);
            var jsonRoot = doc.RootElement;
            var eventName = jsonRoot.GetProperty("type").GetString().MustNotNull();
            var body = jsonRoot.GetProperty("body");
            var requestId = (eventName == TraqBotEvents.Error) ? null : jsonRoot.GetProperty("reqId").GetString();
            return (requestId, eventName, body);
        }
    }
}
