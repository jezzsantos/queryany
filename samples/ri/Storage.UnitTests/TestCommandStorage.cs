using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Storage.UnitTests
{
    public class TestCommandStorage : GenericCommandStorage<TestEntity>
    {
        public TestCommandStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository) : base(logger,
            domainFactory, repository)
        {
        }

        protected override string ContainerName => "acontainername";
    }
}