namespace Common
{
    public static class BooleanExtensions
    {
        public static string ToLower(this bool value)
        {
            return value.ToString().ToLowerInvariant();
        }

        public static string ToUpper(this bool value)
        {
            return value.ToString().ToUpperInvariant();
        }
    }
}