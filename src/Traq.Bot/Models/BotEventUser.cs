using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventUser
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        [NotNull]
        public string? Name { get; set; }

        [JsonPropertyName("displayId")]
        [NotNull]
        public string? DisplayName { get; set; }

        [JsonPropertyName("iconId")]
        public Guid IconFileId { get; set; }

        [JsonPropertyName("bot")]
        public bool IsBot { get; set; }
    }
}
