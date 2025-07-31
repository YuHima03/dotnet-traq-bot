using System.Text.Json.Serialization;
using Traq.Bot.Models;

namespace Traq.Bot
{
    /// <summary>
    /// Provides metadata for JSON serialization of bot event models.
    /// </summary>
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = false)]
    [JsonSerializable(typeof(BotEventArgs))]
    [JsonSerializable(typeof(BotEventChannel))]
    [JsonSerializable(typeof(BotEventDeletedDirectMessage))]
    [JsonSerializable(typeof(BotEventDeletedMessage))]
    [JsonSerializable(typeof(BotEventEmbedding))]
    [JsonSerializable(typeof(BotEventMessage))]
    [JsonSerializable(typeof(BotEventMessageStamp))]
    [JsonSerializable(typeof(BotEventUser))]
    [JsonSerializable(typeof(BotEventUserGroup))]
    [JsonSerializable(typeof(BotEventUserGroupMember))]
    [JsonSerializable(typeof(BotMessageStampsUpdatedEventArgs))]
    [JsonSerializable(typeof(ChannelCreatedEventArgs))]
    [JsonSerializable(typeof(ChannelTopicChangedEventArgs))]
    [JsonSerializable(typeof(DirectMessageDeletedEventArgs))]
    [JsonSerializable(typeof(JoinOrLeftEventArgs))]
    [JsonSerializable(typeof(MessageCreatedOrUpdatedEventArgs))]
    [JsonSerializable(typeof(MessageDeletedEventArgs))]
    [JsonSerializable(typeof(PingEventArgs))]
    [JsonSerializable(typeof(StampCreatedEventArgs))]
    [JsonSerializable(typeof(TagEventArgs))]
    [JsonSerializable(typeof(UserCreatedOrActivatedEventArgs))]
    [JsonSerializable(typeof(UserGroupCreatedEventArgs))]
    [JsonSerializable(typeof(UserGroupMemberEventArgs))]
    [JsonSerializable(typeof(UserGroupUpdatedOrDeletedEventArgs))]
    public partial class SerializationContext : JsonSerializerContext { }
}
