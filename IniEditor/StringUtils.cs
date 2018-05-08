using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IniEditor
{
    public static class StringUtils
    {
        public static string Ellipsis(this string value, int length)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= length) return value;
            return value.Substring(0, length) + "...";
        }

        public static bool IgnoreCaseEqual(this string a, string b)
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsTrue(this string value)
        {
            return value.IgnoreCaseEqual("true") || value.IgnoreCaseEqual("yes") || value.IgnoreCaseEqual("1");
        }


        public static T ToEnum<T>(this string value, T def = default(T)) where T : struct
        {
            return Enum.TryParse(value, out T result) ? result : def;
        }

        public static IEnumerable<string> SplitAndTrim(this string value, char[] separators = null, params char[] trimChars)
        {
            return value.Split(separators ?? new[] {','}).Select(x => x.Trim(trimChars)).Where(x => x.Length > 0);
        }

        public static Color? ToColor(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            try
            {
                return value.StartsWith("#") ? ColorTranslator.FromHtml(value) : Color.FromName(value);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
