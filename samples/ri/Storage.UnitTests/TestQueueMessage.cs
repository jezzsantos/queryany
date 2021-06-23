using QueryAny;

namespace Storage.UnitTests
{
    [EntityName("aqueuename")]
    public class TestQueueMessage
    {
        public string AStringProperty { get; set; }

        public bool ABooleanValue { get; set; }

        public double ADoubleValue { get; set; }
    }
}