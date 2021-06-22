namespace Common
{
    public static class Timezones
    {
        public static string Default = "Pacific/Auckland";
        public static string NewZealand = Default;

#if TESTINGONLY
        public static string Test = "TestTimezone";
#endif
    }
}