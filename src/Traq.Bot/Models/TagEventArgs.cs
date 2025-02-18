using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class TagEventArgs : BotEventArgs
    {
        [JsonPropertyName("tagId")]
        public Guid TagId { get; set; }

        [JsonPropertyName("tag")]
        [NotNull]
        public string? TagName { get; set; }
    }
}
