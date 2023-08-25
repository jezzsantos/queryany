using System;

namespace QueryAny.UnitTests
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UnnamedTestEntity : IQueryableEntity
    {
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class UnnamedTestEntityUnconventionalNamed : IQueryableEntity
    {
    }

    [EntityName("aname")]

    // ReSharper disable once ClassNeverInstantiated.Global
    public class NamedTestEntity : IQueryableEntity
    {
        public string AStringProperty => null;

        public DateTime ADateTimeProperty => default;
    }

    [EntityName("first")]

    // ReSharper disable once ClassNeverInstantiated.Global
    public class FirstTestEntity : IQueryableEntity
    {
        public string AFirstStringProperty => null;

        public DateTime AFirstDateTimeProperty => DateTime.MinValue;
    }

    [EntityName("second")]

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SecondTestEntity : IQueryableEntity
    {
        public string ASecondStringProperty => null;

        public DateTime ASecondDateTimeProperty => DateTime.MinValue;
    }

    [EntityName("third")]

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ThirdTestEntity : IQueryableEntity
    {
        public string AThirdStringProperty => null;

        public DateTime AThirdDateTimeProperty => DateTime.MinValue;
    }
}