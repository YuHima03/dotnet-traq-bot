using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Traq.Bot.Helpers
{
    internal static class Must
    {
        public static T MustNotNull<T>([NotNull] this T? nullable) where T : class
        {
            ArgumentNullException.ThrowIfNull(nullable);
            return nullable;
        }

        public static T MustNotNull<T>([NotNull] this T? nullable) where T : struct
        {
            if (!nullable.HasValue)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(nullable));
            }
            return nullable.Value;
        }
    }
}
