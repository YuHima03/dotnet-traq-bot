using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public class BotEventArgs : EventArgs
    {
        [JsonPropertyName("eventTime")]
        public DateTimeOffset DispatchedAt { get; set; }
    }
}
