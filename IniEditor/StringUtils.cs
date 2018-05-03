namespace IniEditor
{
    public static class StringUtils
    {
        public static string Ellipsis(this string value, int length)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= length) return value;
            return value.Substring(0, length) + "...";
        }
    }
}
