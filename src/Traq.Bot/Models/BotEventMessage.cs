using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventMessage
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("user")]
        [NotNull]
        public BotEventUser? Author { get; set; }

        [JsonPropertyName("channelId")]
        public Guid ChannelId { get; set; }

        [JsonPropertyName("text")]
        [NotNull]
        public string? Text { get; set; }

        [JsonPropertyName("plainText")]
        [NotNull]
        public string? PlainText { get; set; }

        [JsonPropertyName("embedded")]
        [NotNull]
        public BotEventEmbedding[]? Embeddings { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
