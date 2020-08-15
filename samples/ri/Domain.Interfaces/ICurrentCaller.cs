namespace Domain.Interfaces
{
    public interface ICurrentCaller
    {
        string Id { get; }

        string[] Roles { get; }
    }

    public static class CurrentCallerConstants
    {
        public const string AnonymousUserId = "00000000-9999-9999-9999-000000000000";
        public const string AnonymousUserName = "anonymous";
    }
}