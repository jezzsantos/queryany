using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Storage.UnitTests
{
    public class TestStorage : GenericStorage<TestEntity>
    {
        public TestStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository) : base(logger,
            domainFactory, repository)
        {
        }

        protected override string ContainerName => "acontainername";
    }
}