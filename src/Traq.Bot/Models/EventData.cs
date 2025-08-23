using System.Text.Json;
using System.Text.Json.Serialization;

namespace Traq.Bot.Models
{
    /// <summary>
    /// Represents the data of an event received from the traQ service.
    /// </summary>
    public readonly struct EventData
    {
        /// <summary>
        /// The event body that contains event-specific data.
        /// </summary>
        [JsonPropertyName("body")]
        public JsonElement Body { get; init; }

        /// <summary>
        /// The event name.
        /// </summary>
        [JsonPropertyName("type")]
        public string EventName { get; init; }

        /// <summary>
        /// The request ID.
        /// </summary>
        [JsonPropertyName("reqId")]
        public string? RequestId { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventData"/> struct.
        /// </summary>
        [JsonConstructor]
        public EventData(string? requestId, string eventName, JsonElement body)
        {
            (Body, EventName, RequestId) = (body, eventName, requestId);
        }

        /// <summary>
        /// Deconstructs the current instance into its component parts.
        /// </summary>
        public void Deconstruct(out JsonElement body, out string eventName, out string? requestId)
        {
            (body, eventName, requestId) = (Body, EventName, RequestId);
        }
    }
}
