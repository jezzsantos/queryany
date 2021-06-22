using System;
using Xunit;

namespace Storage.IntegrationTests
{
    public class InProcessInMemRepositorySpecSetup : IDisposable
    {
        public InProcessInMemRepositorySpecSetup()
        {
            Repository = new InProcessInMemRepository();
            Blobository = new InProcessInMemRepository();
        }

        public IRepository Repository { get; }

        public IBlobository Blobository { get; }

        public void Dispose()
        {
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class InProcessInMemRepositorySpec : AnyRepositoryBaseSpec, IClassFixture<InProcessInMemRepositorySpecSetup>
    {
        public InProcessInMemRepositorySpec(InProcessInMemRepositorySpecSetup setup) : base(setup.Repository)
        {
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class InProcessInMemBlobositorySpec : AnyBlobositoryBaseSpec,
        IClassFixture<InProcessInMemRepositorySpecSetup>
    {
        public InProcessInMemBlobositorySpec(InProcessInMemRepositorySpecSetup setup) : base(setup.Blobository)
        {
        }
    }
}