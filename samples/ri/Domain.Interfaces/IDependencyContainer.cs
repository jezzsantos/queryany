namespace Domain.Interfaces
{
    public interface IDependencyContainer
    {
        TDependency Resolve<TDependency>();
    }
}