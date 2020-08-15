using Domain.Interfaces;

namespace Api.Common.Auth
{
    public class AnonymousCaller : ICurrentCaller
    {
        public string Id => CurrentCallerConstants.AnonymousUserId;

        public string[] Roles => new string[] { };
    }
}