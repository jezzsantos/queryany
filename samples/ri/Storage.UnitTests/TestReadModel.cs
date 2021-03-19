using QueryAny;
using Storage.Interfaces.ReadModels;

namespace Storage.UnitTests
{
    [EntityName("acontainername")]
    public class TestReadModel : ReadModelEntity
    {
        public string APropertyName { get; set; }
    }
}