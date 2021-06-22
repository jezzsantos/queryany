using Application.Storage.Interfaces.ReadModels;
using QueryAny;

namespace Storage.UnitTests
{
    [EntityName("acontainername")]
    public class TestReadModel : ReadModelEntity
    {
        public string APropertyName { get; set; }
    }
}