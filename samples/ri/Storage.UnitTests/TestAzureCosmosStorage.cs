namespace Storage.UnitTests
{
    public class TestAzureCosmosStorage : AzureCosmosStorage<TestEntity>
    {
        public TestAzureCosmosStorage(IAzureCosmosConnection connection) : base(connection)
        {
        }

        protected override string ContainerName => "TestEntities";
    }
}