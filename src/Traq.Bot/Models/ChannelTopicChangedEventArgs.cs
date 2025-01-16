using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class ChannelTopicChangedEventArgs : BotEventArgs
    {
        [JsonPropertyName("channel")]
        [NotNull]
        public BotEventChannel? Channel { get; set; }

        [JsonPropertyName("topic")]
        [NotNull]
        public string? Topic { get; set; }

        [JsonPropertyName("updator")]
        [NotNull]
        public BotEventUser? UpdatedBy { get; set; }
    }
}
