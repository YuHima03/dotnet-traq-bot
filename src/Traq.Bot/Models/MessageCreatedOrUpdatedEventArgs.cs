using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class MessageCreatedOrUpdatedEventArgs
    {
        [JsonPropertyName("eventTime")]
        public DateTimeOffset DispatchedAt { get; set; }

        [JsonPropertyName("message")]
        [NotNull]
        public BotEventMessage? Message { get; set; }
    }
}
