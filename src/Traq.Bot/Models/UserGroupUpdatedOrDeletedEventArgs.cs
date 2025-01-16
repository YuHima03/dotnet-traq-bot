using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class UserGroupUpdatedOrDeletedEventArgs : BotEventArgs
    {
        [JsonPropertyName("groupId")]
        public Guid GroupId { get; set; }
    }
}
