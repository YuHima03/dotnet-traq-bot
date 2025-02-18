using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class UserGroupMemberEventArgs : BotEventArgs
    {
        [JsonPropertyName("groupMember")]
        [NotNull]
        public BotEventUserGroupMember? GroupMember { get; set; }
    }
}
