namespace Domain.Interfaces
{
    public interface ICurrentCaller
    {
        string Id { get; }

        string[] Roles { get; }
    }

    public static class CurrentCallerConstants
    {
        public const string AnonymousUserId = "xxx_0000000000000000000000";
        public const string AnonymousUserName = "anonymous";
    }
}