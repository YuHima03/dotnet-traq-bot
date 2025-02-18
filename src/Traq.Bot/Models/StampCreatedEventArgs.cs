using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class StampCreatedEventArgs : BotEventArgs
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        [NotNull]
        public string? Name { get; set; }

        [JsonPropertyName("fileId")]
        public Guid FileId { get; set; }

        [JsonPropertyName("creator")]
        [NotNull]
        public BotEventUser? CreatedBy { get; set; }
    }
}
