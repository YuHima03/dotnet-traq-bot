using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public class DirectMessageDeletedEventArgs
    {
        [JsonPropertyName("eventTime")]
        public DateTimeOffset DispatchedAt { get; set; }

        [JsonPropertyName("message")]
        [NotNull]
        public BotEventDeletedDirectMessage? Message { get; set; }
    }
}
