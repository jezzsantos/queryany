using Services.Interfaces;

namespace CarsApi.Auth
{
    public class FakeCaller : ICurrentCaller
    {
        public string Id => "anonymous";

        public string[] Roles => new[] {"normaluser"};
    }
}