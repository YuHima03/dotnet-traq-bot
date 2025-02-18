using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventEmbedding
    {
        [JsonPropertyName("raw")]
        [NotNull]
        public string? RawText { get; set; }

        [JsonPropertyName("type")]
        [NotNull]
        public string? Type { get; set; }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }
    }
}
