using Domain.Interfaces;
using Funq;
using QueryAny.Primitives;

namespace Api.Common
{
    public class FuncDependencyContainer : IDependencyContainer
    {
        private readonly Container container;

        public FuncDependencyContainer(Container container)
        {
            container.GuardAgainstNull(nameof(container));
            this.container = container;
        }

        public TDependency Resolve<TDependency>()
        {
            return this.container.Resolve<TDependency>();
        }
    }
}