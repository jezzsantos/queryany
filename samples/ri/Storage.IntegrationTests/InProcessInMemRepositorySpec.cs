using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;

namespace Storage.IntegrationTests
{
    [TestClass, TestCategory("Integration.Storage")]
    public class InProcessInMemRepositorySpec : AnyRepositoryBaseSpec
    {
        private static InProcessInMemRepository repository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            repository = new InProcessInMemRepository();
        }

        protected override RepoInfo GetRepository<TEntity>()
        {
            return new RepoInfo
            {
                Repository = repository,
                ContainerName = typeof(TEntity).GetEntityNameSafe()
            };
        }
    }
}