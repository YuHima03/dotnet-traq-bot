using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventDeletedDirectMessage
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("channelId")]
        public Guid ChannelId { get; set; }
    }
}
