using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventChannel
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        [NotNull]
        public string? Name { get; set; }

        [JsonPropertyName("path")]
        [NotNull]
        public string? Path { get; set; }

        [JsonPropertyName("parentId")]
        public Guid ParentChannelId { get; set; }

        [JsonPropertyName("creator")]
        [NotNull]
        public BotEventUser? CreatedBy { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
