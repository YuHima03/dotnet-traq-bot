using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Traq.Bot.Http.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to build traQ BOT (HTTP mode).
    /// </summary>
    public static class EndpointRouteBuilderExtension
    {
        const string RequiresUnreferencedOrDynamicCodeMessage = "This method calls a method that may use System.Reflection.";

        /// <summary>
        /// Maps a route to handle events for a specified traQ BOT type.
        /// </summary>
        /// <typeparam name="TBot"></typeparam>
        /// <param name="builder"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        [RequiresUnreferencedCode(RequiresUnreferencedOrDynamicCodeMessage)]
        [RequiresDynamicCode(RequiresUnreferencedOrDynamicCodeMessage)]
        public static IEndpointRouteBuilder MapTraqBot<TBot>(this IEndpointRouteBuilder builder, [StringSyntax("Route")] string pattern) where TBot : TraqHttpBot
        {
            builder.Map(pattern, EndpointHandler<TBot>.HandleEvent);
            return builder;
        }

        static class EndpointHandler<TBot> where TBot : TraqHttpBot
        {
            public static Results<NoContent, BadRequest> HandleEvent(
                [FromServices] TBot bot,
                [FromHeader(Name = TraqHttpBot.HeaderEventName)] string eventName,
                [FromHeader(Name = TraqHttpBot.HeaderRequestId)] string? requestId,
                [FromHeader(Name = TraqHttpBot.HeaderVerificationToken)] string? verificationTokenChallenge,
                [FromBody] JsonElement body)
            {
                return bot.HandleEvent(eventName, requestId, verificationTokenChallenge, body);
            }
        }
    }
}
