namespace Common
{
    public static class NumberExtensions
    {
        public static bool IsInclusiveBetween(this int number, int min, int max)
        {
            return number <= max && number >= min;
        }
    }
}