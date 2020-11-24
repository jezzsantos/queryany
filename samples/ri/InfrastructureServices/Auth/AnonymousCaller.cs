using Domain.Interfaces;

namespace InfrastructureServices.Auth
{
    public class AnonymousCaller : ICurrentCaller
    {
        public string Id => CurrentCallerConstants.AnonymousUserId;

        public string[] Roles => new string[] { };
    }
}