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
        /// <inheritdoc />
        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await InitializeAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var data = await WaitForNextEventAsync(stoppingToken);
                    await HandleEventAsync(data, stoppingToken);
                }
            }
            catch (OperationCanceledException ex) when (stoppingToken.IsCancellationRequested)
            {
                logger?.LogWarning(ex, "The operation is cancelled because cancellation is requested.");
            }
        }

        ValueTask HandleEventAsync(EventData data, CancellationToken ct)
        {
            var (body, eventName, _) = data;
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

        /// <summary>
        /// Triggered once when the bot is ready for starting.
        /// </summary>
        protected virtual ValueTask InitializeAsync(CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>JOIN</c> event is received.
        /// </summary>
        protected virtual ValueTask OnJoinedAsync(JoinOrLeftEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>LEFT</c> event is received.
        /// </summary>
        protected virtual ValueTask OnLeftAsync(JoinOrLeftEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>PING</c> event is received.
        /// </summary>
        protected virtual ValueTask OnPingAsync(PingEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>MESSAGE_CREATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnMessageCreatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>MESSAGE_DELETED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnMessageDeletedAsync(MessageDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>MESSAGE_UPDATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnMessageUpdatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>DIRECT_MESSAGE_CREATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnDirectMessageCreatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>DIRECT_MESSAGE_DELETED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnDirectMessageDeletedAsync(DirectMessageDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>DIRECT_MESSAGE_UPDATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnDirectMessageUpdatedAsync(MessageCreatedOrUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>MESSAGE_STAMPS_UPDATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnBotMessageStampsUpdatedAsync(BotMessageStampsUpdatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>CHANNEL_CREATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnChannelCreatedAsync(ChannelCreatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>CHANNEL_TOPIC_CHANGED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnChannelTopicChangedAsync(ChannelTopicChangedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_CREATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserCreatedAsync(UserCreatedOrActivatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_ACTIVATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserActivatedAsync(UserCreatedOrActivatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_CREATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupCreatedAsync(UserGroupCreatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_UPDATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupUpdatedAsync(UserGroupUpdatedOrDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_DELETED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupDeletedAsync(UserGroupUpdatedOrDeletedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_MEMBER_ADDED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupMemberAddedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_MEMBER_UPDATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupMemberUpdatedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_MEMBER_REMOVED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupMemberRemovedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_ADMIN_ADDED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupAdminAddedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>USER_GROUP_ADMIN_REMOVED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnUserGroupAdminRemovedAsync(UserGroupMemberEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>STAMP_CREATED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnStampCreatedAsync(StampCreatedEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>TAG_ADDED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnTagAddedAsync(TagEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <summary>
        /// Triggered when a <c>TAG_REMOVED</c> event is received.
        /// </summary>
        protected virtual ValueTask OnTagRemovedAsync(TagEventArgs args, CancellationToken ct) => ValueTask.CompletedTask;

        /// <inheritdoc />
        public sealed override Task StartAsync(CancellationToken cancellationToken) => base.StartAsync(cancellationToken);

        /// <summary>
        /// Returns a <see cref="ValueTask{TResult}"/> that completes when the next event is received.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that provides received event data.</returns>
        protected abstract ValueTask<EventData> WaitForNextEventAsync(CancellationToken ct);
    }
}
