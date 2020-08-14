using Domain.Interfaces;

namespace Api.Common.Auth
{
    public class FakeCaller : ICurrentCaller
    {
        public string Id => "anonymous";

        public string[] Roles => new[] {"normaluser"};
    }
}