using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class BotEventUserGroup
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        [NotNull]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        [NotNull]
        public string? Description { get; set; }

        [JsonPropertyName("type")]
        [NotNull]
        public string? Type { get; set; }

        [JsonPropertyName("icon")]
        public Guid IconFileId { get; set; }

        [JsonPropertyName("admins")]
        [NotNull]
        public BotEventUserGroupMember[]? Admins { get; set; }

        [JsonPropertyName("members")]
        [NotNull]
        public BotEventUserGroupMemberWithRole[]? Members { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class BotEventUserGroupMemberWithRole
    {
        [JsonPropertyName("groupId")]
        public Guid GroupId { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }
}
