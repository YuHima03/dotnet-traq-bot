using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventMessageStamp
    {
        [JsonPropertyName("stampId")]
        public Guid StampId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
