using QueryAny;

namespace Storage.UnitTests
{
    public class TestQueryEntity : IQueryableEntity
    {
        public string AStringValue { get; set; }

        public bool ABooleanValue { get; set; }

        public double ADoubleValue { get; set; }
    }
}