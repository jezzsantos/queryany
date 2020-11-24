using Domain.Interfaces;

namespace InfrastructureServices.Identity
{
    public class AnonymousCaller : ICurrentCaller
    {
        public string Id => CurrentCallerConstants.AnonymousUserId;

        public string[] Roles => new string[] { };
    }
}