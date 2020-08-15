using Domain.Interfaces;

namespace Api.Common.Auth
{
    public class FakeCaller : ICurrentCaller
    {
        public string Id => CurrentCallerConstants.AnonymousUserId;

        public string[] Roles => new string[] { };
    }
}