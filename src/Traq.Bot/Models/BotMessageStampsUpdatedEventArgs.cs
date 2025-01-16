using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotMessageStampsUpdatedEventArgs
    {
        [JsonPropertyName("eventTime")]
        public DateTime DispatchedAt { get; set; }

        [JsonPropertyName("messageId")]
        public Guid MessageId { get; set; }

        [JsonPropertyName("stamps")]
        public BotEventMessageStamp[]? Stamps { get; set; }
    }
}
