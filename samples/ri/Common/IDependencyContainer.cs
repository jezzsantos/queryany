namespace Common
{
    public interface IDependencyContainer
    {
        TDependency Resolve<TDependency>();
    }
}