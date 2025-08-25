using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Traq.Bot.Http.Helpers;
using Traq.Bot.Models;

namespace Traq.Bot.Http
{
    /// <summary>
    /// A base class for implementing a traQ BOT service that receives messages over HTTP requests.
    /// </summary>
    /// <param name="verificationToken"></param>
    /// <param name="logger"></param>
    /// <param name="baseLogger"></param>
    public abstract class TraqHttpBot(
        string? verificationToken,
        ILogger<TraqHttpBot> logger,
        ILogger<TraqBot> baseLogger
        )
        : TraqBot(baseLogger)
    {
        readonly ConcurrentAwaitableQueue<EventData> _eventDataQueue = new();

        /// <summary>
        /// Handler for incoming HTTP requests from traQ service.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="requestId"></param>
        /// <param name="verificationTokenChallenge"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public Results<NoContent, BadRequest> HandleEvent(string eventName, string? requestId, string? verificationTokenChallenge, JsonElement body)
        {
            if (!string.Equals(verificationToken, verificationTokenChallenge, StringComparison.Ordinal))
            {
                logger.LogWarning("Invalid verification token detected.");
                return TypedResults.BadRequest();
            }
            _eventDataQueue.Enqueue(new EventData(requestId, eventName, body));
            return TypedResults.NoContent();
        }

        /// <inheritdoc />
        protected sealed override ValueTask<EventData> WaitForNextEventAsync(CancellationToken ct)
        {
            return _eventDataQueue.DequeueAsync(ct);
        }
    }
}
