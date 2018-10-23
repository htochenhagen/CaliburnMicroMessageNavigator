using System;

namespace CaliburnMicroMessageNavigator.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Substring(0, Math.Min(value.Length, 100)).Replace("\n", string.Empty)
                .Replace("\r", string.Empty);
        }
    }
}