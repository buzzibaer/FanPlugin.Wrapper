using System.Globalization;

namespace FanPlugin.Wrapper
{
    internal static class FanVideoId
    {
        internal static bool TryParse(string value, out int videoId)
        {
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out videoId)
                && videoId >= 0
                && videoId <= 99;
        }
    }
}
