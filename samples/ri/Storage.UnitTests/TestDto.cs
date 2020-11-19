using QueryAny;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [EntityName("acontainername")]
    public class TestDto : IQueryableEntity
    {
        public string AStringValue { get; set; }
    }

    [EntityName("acontainername")]
    public class TestDtoWithId : IQueryableEntity, IHasIdentity
    {
        public string AStringValue { get; set; }

        public string Id { get; set; }
    }
}