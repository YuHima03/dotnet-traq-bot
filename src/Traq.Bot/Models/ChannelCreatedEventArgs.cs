﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    public sealed class ChannelCreatedEventArgs : BotEventArgs
    {
        [JsonPropertyName("channel")]
        [NotNull]
        public BotEventChannel? Channel { get; set; }
    }
}
