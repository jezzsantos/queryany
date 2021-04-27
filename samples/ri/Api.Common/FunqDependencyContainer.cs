using Domain.Interfaces;
using Funq;

namespace Api.Common
{
    public class FunqDependencyContainer : IDependencyContainer
    {
        private readonly Container container;

        public FunqDependencyContainer(Container container)
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