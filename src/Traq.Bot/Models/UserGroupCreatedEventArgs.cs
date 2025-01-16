using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class UserGroupCreatedEventArgs : BotEventArgs
    {
        [JsonPropertyName("group")]
        [NotNull]
        public BotEventUserGroup? Group { get; set; }
    }
}
