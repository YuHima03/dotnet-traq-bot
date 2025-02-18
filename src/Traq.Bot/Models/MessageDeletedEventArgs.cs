using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class MessageDeletedEventArgs
    {
        [JsonPropertyName("eventTime")]
        public DateTimeOffset DispatchedAt { get; set; }

        [JsonPropertyName("message")]
        [NotNull]
        public BotEventDeletedMessage? Message { get; set; }
    }
}
