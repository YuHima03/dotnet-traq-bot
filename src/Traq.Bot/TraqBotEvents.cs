namespace Traq.Bot
{
    /// <summary>
    /// Provides event names for the traQ bot.
    /// </summary>
    public static class TraqBotEvents
    {
#pragma warning disable CS1591

        public const string Error = "ERROR";

        #region System Events
        public const string Join = "JOIN";

        public const string Left = "LEFT";

        public const string Ping = "PING";
        #endregion

        #region Message Events
        public const string MessageCreated = "MESSAGE_CREATED";

        public const string MessageDeleted = "MESSAGE_DELETED";

        public const string MessageUpdated = "MESSAGE_UPDATED";

        public const string DirectMessageCreated = "DIRECT_MESSAGE_CREATED";

        public const string DirectMessageDeleted = "DIRECT_MESSAGE_DELETED";

        public const string DirectMessageUpdated = "DIRECT_MESSAGE_UPDATED";

        public const string BotMessageStampsUpdated = "BOT_MESSAGE_STAMPS_UPDATED";
        #endregion

        #region User Events
        public const string UserCreated = "USER_CREATED";

        public const string UserActivated = "USER_ACTIVATED";
        #endregion

        #region User Group Events
        public const string UserGroupAdminAdded = "USER_GROUP_ADMIN_ADDED";

        public const string UserGroupAdminRemoved = "USER_GROUP_ADMIN_REMOVED";

        public const string UserGroupCreated = "USER_GROUP_CREATED";

        public const string UserGroupUpdated = "USER_GROUP_UPDATED";

        public const string UserGroupDeleted = "USER_GROUP_DELETED";

        public const string UserGroupMemberAdded = "USER_GROUP_MEMBER_ADDED";

        public const string UserGroupMemberRemoved = "USER_GROUP_MEMBER_REMOVED";

        public const string UserGroupMemberUpdated = "USER_GROUP_MEMBER_UPDATED";
        #endregion

        #region Channel Events
        public const string ChannelCreated = "CHANNEL_CREATED";

        public const string ChannelTopicChanged = "CHANNEL_TOPIC_CHANGED";
        #endregion

        #region Stamp Events
        public const string StampCreated = "STAMP_CREATED";
        #endregion

        #region Tag Events
        public const string TagAdded = "TAG_ADDED";

        public const string TagRemoved = "TAG_REMOVED";
        #endregion

#pragma warning restore CS1591
    }
}
