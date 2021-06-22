namespace Application.Interfaces
{
    public interface ICurrentCaller
    {
        string Id { get; }

        string[] Roles { get; }
    }
}