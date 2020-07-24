using QueryAny;

namespace CarsDomain.UnitTests
{
    public class TestEntity : IQueryableEntity
    {
        public string APropertyName { get; set; }
    }
}