using Application.Interfaces;
using Domain.Interfaces;

namespace InfrastructureServices.Identity
{
    public class AnonymousCaller : ICurrentCaller
    {
        public string Id => CallerConstants.AnonymousUserId;

        public string[] Roles => new string[] { };
    }
}