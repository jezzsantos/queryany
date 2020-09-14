using QueryAny;
using Storage.Interfaces.ReadModels;

namespace Storage.UnitTests
{
    [EntityName("acontainername")]
    public class TestReadModel : IReadModelEntity
    {
        public string APropertyName { get; set; }

        public string Id { get; set; }
    }
}