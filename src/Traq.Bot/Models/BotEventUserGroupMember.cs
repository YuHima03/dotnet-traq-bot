using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventUserGroupMember
    {
        [JsonPropertyName("groupId")]
        public Guid GroupId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }
    }
}
