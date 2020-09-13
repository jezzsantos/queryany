using QueryAny;

namespace Storage.UnitTests
{
    [EntityName("acontainername")]
    public class TestDto : IQueryableEntity
    {
        public string AStringValue { get; set; }
    }
}