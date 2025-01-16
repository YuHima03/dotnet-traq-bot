using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class UserCreatedOrActivatedEventArgs : BotEventArgs
    {
        [JsonPropertyName("user")]
        [NotNull]
        public BotEventUser? User { get; set; }
    }
}
