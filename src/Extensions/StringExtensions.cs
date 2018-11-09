using System;
using System.Diagnostics;
using System.IO;

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

        public static void Log(this string @this)
        {
            var msg = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss:fff}: {@this}";
            var fileName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            Debug.WriteLine($"{fileName} --> {msg}");
            Trace.TraceInformation(msg);
        }

    }
}