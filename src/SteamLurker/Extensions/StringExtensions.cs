using System;
using System.Linq;

namespace SteamLurker.Extensions
{
    public static class StringExtensions
    {
        public static string GetLineAfter(this string value, string marker)
        {
            var index = value.IndexOf(marker);
            if (index == -1)
            {
                return string.Empty;
            }

            var textAfter = value.Substring(index + marker.Length);
            var lines = textAfter.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            return lines.First().Trim();
        }
    }
}
