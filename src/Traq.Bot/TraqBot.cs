using Microsoft.Extensions.DependencyInjection;
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
    /// <param name="provider"></param>
    public abstract class TraqBot(IServiceProvider provider) : BackgroundService
    {
        readonly ILogger<TraqBot> _logger = provider.GetRequiredService<ILogger<TraqBot>>();

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
                _logger.LogWarning(ex, "The operation is cancelled because cancellation is requested.");
            }
        }

        ValueTask HandleEventAsync(string? requestId, string eventName, JsonElement body, CancellationToken ct)
        {
            switch (eventName)
            {
                #region System Events
                case "JOIN":
                    return OnJoinedAsync(body.Deserialize<JoinOrLeftEventArgs>().MustNotNull(), ct);
                case "LEFT":
                    return OnLeftAsync(body.Deserialize<JoinOrLeftEventArgs>().MustNotNull(), ct);
                case "PING":
                    return OnPingAsync(body.Deserialize<PingEventArgs>().MustNotNull(), ct);
                #endregion

                #region Message Events
                case "MESSAGE_CREATED":
                    return OnMessageCreatedAsync(body.Deserialize<MessageCreatedOrUpdatedEventArgs>().MustNotNull(), ct);
                case "MESSAGE_DELETED":
                    return OnMessageDeletedAsync(body.Deserialize<MessageDeletedEventArgs>().MustNotNull(), ct);
                case "MESSAGE_UPDATED":
                    return OnMessageUpdatedAsync(body.Deserialize<MessageCreatedOrUpdatedEventArgs>().MustNotNull(), ct);
                case "DIRECT_MESSAGE_CREATED":
                    return OnDirectMessageCreatedAsync(body.Deserialize<MessageCreatedOrUpdatedEventArgs>().MustNotNull(), ct);
                case "DIRECT_MESSAGE_DELETED":
                    return OnDirectMessageDeletedAsync(body.Deserialize<DirectMessageDeletedEventArgs>().MustNotNull(), ct);
                case "DIRECT_MESSAGE_UPDATED":
                    return OnDirectMessageUpdatedAsync(body.Deserialize<MessageCreatedOrUpdatedEventArgs>().MustNotNull(), ct);
                case "BOT_MESSAGE_STAMPS_UPDATED":
                    return OnBotMessageStampsUpdatedAsync(body.Deserialize<BotMessageStampsUpdatedEventArgs>().MustNotNull(), ct);
                #endregion

                #region Channel Events
                case "CHANNEL_CREATED":
                    return OnChannelCreatedAsync(body.Deserialize<ChannelCreatedEventArgs>().MustNotNull(), ct);
                case "CHANNEL_TOPIC_CHANGED":
                    return OnChannelTopicChangedAsync(body.Deserialize<ChannelTopicChangedEventArgs>().MustNotNull(), ct);
                #endregion

                #region User Events
                case "USER_CREATED":
                    return OnUserCreatedAsync(body.Deserialize<UserCreatedOrActivatedEventArgs>().MustNotNull(), ct);
                case "USER_ACTIVATED":
                    return OnUserActivatedAsync(body.Deserialize<UserCreatedOrActivatedEventArgs>().MustNotNull(), ct);
                #endregion

                #region UserGroup Events
                case "USER_GROUP_CREATED":
                    return OnUserGroupCreatedAsync(body.Deserialize<UserGroupCreatedEventArgs>().MustNotNull(), ct);
                case "USER_GROUP_UPDATED":
                    return OnUserGroupUpdatedAsync(body.Deserialize<UserGroupUpdatedOrDeletedEventArgs>().MustNotNull(), ct);
                case "USER_GROUP_DELETED":
                    return OnUserGroupDeletedAsync(body.Deserialize<UserGroupUpdatedOrDeletedEventArgs>().MustNotNull(), ct);
                case "USER_GROUP_MEMBER_ADDED":
                    return OnUserGroupMemberAddedAsync(body.Deserialize<UserGroupMemberEventArgs>().MustNotNull(), ct);
                case "USER_GROUP_MEMBER_UPDATED":
                    return OnUserGroupMemberUpdatedAsync(body.Deserialize<UserGroupMemberEventArgs>().MustNotNull(), ct);
                case "USER_GROUP_MEMBER_REMOVED":
                    return OnUserGroupMemberRemovedAsync(body.Deserialize<UserGroupMemberEventArgs>().MustNotNull(), ct);
                case "USER_GROUP_ADMIN_ADDED":
                    return OnUserGroupMemberAddedAsync(body.Deserialize<UserGroupMemberEventArgs>().MustNotNull(), ct);
                case "USER_GROUP_ADMIN_REMOVED":
                    return OnUserGroupMemberAddedAsync(body.Deserialize<UserGroupMemberEventArgs>().MustNotNull(), ct);
                #endregion

                #region Stamp Events
                case "STAMP_CREATED":
                    return OnStampCreatedAsync(body.Deserialize<StampCreatedEventArgs>().MustNotNull(), ct);
                #endregion

                #region Tag Events
                case "TAG_ADDED":
                    return OnTagAddedAsync(body.Deserialize<TagEventArgs>().MustNotNull(), ct);
                case "TAG_REMOVED":
                    return OnTagRemovedAsync(body.Deserialize<TagEventArgs>().MustNotNull(), ct);
                #endregion

                default:
                    _logger.LogWarning("Unknown event name: {}", eventName);
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
