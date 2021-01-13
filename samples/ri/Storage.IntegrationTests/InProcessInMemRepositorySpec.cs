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

    [TestClass, TestCategory("Integration.Storage")]
    public class InProcessInMemBlobositorySpec : AnyBlobositoryBaseSpec
    {
        private static InProcessInMemRepository blobository;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            InitializeAllTests();
            blobository = new InProcessInMemRepository();
        }

        protected override BloboInfo GetBlobository<TEntity>()
        {
            return new BloboInfo
            {
                Blobository = blobository,
                ContainerName = typeof(TEntity).GetEntityNameSafe()
            };
        }
    }
}