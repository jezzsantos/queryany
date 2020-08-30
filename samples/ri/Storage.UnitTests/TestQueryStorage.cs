using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Storage.UnitTests
{
    public class TestQueryStorage : GenericQueryStorage<TestEntity>
    {
        public TestQueryStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository) : base(logger,
            domainFactory, repository)
        {
        }

        protected override string ContainerName => "acontainername";
    }
}