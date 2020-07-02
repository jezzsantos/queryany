namespace Services.Interfaces
{
    public class FakeCaller : ICurrentCaller
    {
        public string Id => "afaekuserid";
        public string[] Roles => new[] {"normaluser"};
    }
}