using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class PingEventArgs
    {
        [JsonPropertyName("eventTime")]
        public DateTime DispatchedAt { get; set; }
    }
}
