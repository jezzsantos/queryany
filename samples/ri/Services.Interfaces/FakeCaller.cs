namespace Services.Interfaces
{
    public class FakeCaller : ICurrentCaller
    {
        public string Id => "anonymous";
        public string[] Roles => new[] {"normaluser"};
    }
}