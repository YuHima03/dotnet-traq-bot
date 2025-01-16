using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventDeletedMessage
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("channelId")]
        public Guid ChannelId { get; set; }
    }
}
