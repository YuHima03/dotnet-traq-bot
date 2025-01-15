using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class JoinOrLeftEventArgs
    {
        [JsonPropertyName("eventTime")]
        public DateTime DispatchedAt { get; set; }

        [JsonPropertyName("channel")]
        [NotNull]
        public BotEventChannel? Channel { get; set; }
    }
}
