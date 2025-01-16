using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class JoinOrLeftEventArgs : BotEventArgs
    {
        [JsonPropertyName("channel")]
        [NotNull]
        public BotEventChannel? Channel { get; set; }
    }
}
