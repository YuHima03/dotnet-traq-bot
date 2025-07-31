using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Traq.Bot.Helpers;
using Traq.Bot.Models;

namespace Traq.Bot
{
    /// <summary>
    /// A base class for implementing a background service that handles events dispatched by a traQ server.
    /// </summary>
    /// <param name="logger"></param>
    public abstract class TraqBot(
        ILogger<TraqBot>? logger
        ) : BackgroundService
    {
        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await InitializeAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var (reqId, evName, body) = await WaitForNextEventAsync(stoppingToken);
                    await HandleEventAsync(reqId, evName, body, stoppingToken);
                }
            }
            catch (OperationCanceledException ex) when (stoppingToken.IsCancellationRequested)
            {
                logger?.LogWarning(ex, "The operation is cancelled because cancellation is requested.");
            }
        }

        ValueTask HandleEventAsync(string? requestId, string eventName, JsonElement body, CancellationToken ct)
        {
            switch (eventName)
            {
                #region System Events
                case TraqBotEvents.Join:
                    return OnJoinedAsync(body.Deserialize(SerializationContext.Default.JoinOrLeftEventArgs).MustNotNull(), ct);
                case TraqBotEvents.Left:
                    return OnLeftAsync(body.Deserialize(SerializationContext.Default.JoinOrLeftEventArgs).MustNotNull(), ct);
                case TraqBotEvents.Ping:
                    return OnPingAsync(body.Deserialize(SerializationContext.Default.PingEventArgs).MustNotNull(), ct);
                #endregion

                #region Message Events
                case TraqBotEvents.MessageCreated:
                    return OnMessageCreatedAsync(body.Deserialize(SerializationContext.Default.MessageCreatedOrUpdatedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.MessageDeleted:
                    return OnMessageDeletedAsync(body.Deserialize(SerializationContext.Default.MessageDeletedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.MessageUpdated:
                    return OnMessageUpdatedAsync(body.Deserialize(SerializationContext.Default.MessageCreatedOrUpdatedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.DirectMessageCreated:
                    return OnDirectMessageCreatedAsync(body.Deserialize(SerializationContext.Default.MessageCreatedOrUpdatedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.DirectMessageDeleted:
                    return OnDirectMessageDeletedAsync(body.Deserialize(SerializationContext.Default.DirectMessageDeletedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.DirectMessageUpdated:
                    return OnDirectMessageUpdatedAsync(body.Deserialize(SerializationContext.Default.MessageCreatedOrUpdatedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.BotMessageStampsUpdated:
                    return OnBotMessageStampsUpdatedAsync(body.Deserialize(SerializationContext.Default.BotMessageStampsUpdatedEventArgs).MustNotNull(), ct);
                #endregion

                #region Channel Events
                case TraqBotEvents.ChannelCreated:
                    return OnChannelCreatedAsync(body.Deserialize(SerializationContext.Default.ChannelCreatedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.ChannelTopicChanged:
                    return OnChannelTopicChangedAsync(body.Deserialize(SerializationContext.Default.ChannelTopicChangedEventArgs).MustNotNull(), ct);
                #endregion

                #region User Events
                case TraqBotEvents.UserCreated:
                    return OnUserCreatedAsync(body.Deserialize(SerializationContext.Default.UserCreatedOrActivatedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserActivated:
                    return OnUserActivatedAsync(body.Deserialize(SerializationContext.Default.UserCreatedOrActivatedEventArgs).MustNotNull(), ct);
                #endregion

                #region UserGroup Events
                case TraqBotEvents.UserGroupCreated:
                    return OnUserGroupCreatedAsync(body.Deserialize(SerializationContext.Default.UserGroupCreatedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserGroupUpdated:
                    return OnUserGroupUpdatedAsync(body.Deserialize(SerializationContext.Default.UserGroupUpdatedOrDeletedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserGroupDeleted:
                    return OnUserGroupDeletedAsync(body.Deserialize(SerializationContext.Default.UserGroupUpdatedOrDeletedEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserGroupMemberAdded:
                    return OnUserGroupMemberAddedAsync(body.Deserialize(SerializationContext.Default.UserGroupMemberEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserGroupMemberUpdated:
                    return OnUserGroupMemberUpdatedAsync(body.Deserialize(SerializationContext.Default.UserGroupMemberEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserGroupMemberRemoved:
                    return OnUserGroupMemberRemovedAsync(body.Deserialize(SerializationContext.Default.UserGroupMemberEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserGroupAdminAdded:
                    return OnUserGroupAdminAddedAsync(body.Deserialize(SerializationContext.Default.UserGroupMemberEventArgs).MustNotNull(), ct);
                case TraqBotEvents.UserGroupAdminRemoved:
                    return OnUserGroupAdminRemovedAsync(body.Deserialize(SerializationContext.Default.UserGroupMemberEventArgs).MustNotNull(), ct);
                #endregion

                #region Stamp Events
                case TraqBotEvents.StampCreated:
                    return OnStampCreatedAsync(body.Deserialize(SerializationContext.Default.StampCreatedEventArgs).MustNotNull(), ct);
                #endregion

                #region Tag Events
                case TraqBotEvents.TagAdded:
                    return OnTagAddedAsync(body.Deserialize(SerializationContext.Default.TagEventArgs).MustNotNull(), ct);
                case TraqBotEvents.TagRemoved:
                    return OnTagRemovedAsync(body.Deserialize(SerializationContext.Default.TagEventArgs).MustNotNull(), ct);
                #endregion

                default:
                    logger?.LogWarning("Unknown event name: {}", eventName);
                    return ValueTask.CompletedTask;
            }
        }

        protected virtual ValueTask InitializeAsync(CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnJoinedAsync(JoinOrLeftEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnLeftAsync(JoinOrLeftEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnPingAsync(PingEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnMessageCreatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnMessageDeletedAsync(MessageDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnMessageUpdatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnDirectMessageCreatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnDirectMessageDeletedAsync(DirectMessageDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnDirectMessageUpdatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnBotMessageStampsUpdatedAsync(BotMessageStampsUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnChannelCreatedAsync(ChannelCreatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnChannelTopicChangedAsync(ChannelTopicChangedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserCreatedAsync(UserCreatedOrActivatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserActivatedAsync(UserCreatedOrActivatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupCreatedAsync(UserGroupCreatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupUpdatedAsync(UserGroupUpdatedOrDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupDeletedAsync(UserGroupUpdatedOrDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupMemberAddedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupMemberUpdatedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupMemberRemovedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupAdminAddedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnUserGroupAdminRemovedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnStampCreatedAsync(StampCreatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnTagAddedAsync(TagEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        protected virtual ValueTask OnTagRemovedAsync(TagEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        public sealed override Task StartAsync(CancellationToken cancellationToken) => base.StartAsync(cancellationToken);

        protected abstract ValueTask<(string? RequestId, string EventName, JsonElement Body)> WaitForNextEventAsync(CancellationToken ct);
    }
}
