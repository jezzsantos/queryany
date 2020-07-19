using System;
using QueryAny.Primitives;
using QueryAny.Properties;

namespace QueryAny
{
    public class ResultOptions
    {
        public const long DefaultLimit = -1;
        public const long DefaultOffset = 0;
        public const OrderDirection DefaultOrderDirection = OrderDirection.Ascending;
        public static readonly string DefaultOrder = null;

        public ResultOptions()
        {
            Limit = DefaultLimit;
            Offset = DefaultOffset;
            Order = new Ordering(DefaultOrder, DefaultOrderDirection);
        }

        public long Limit { get; private set; }
        public long Offset { get; private set; }
        public Ordering Order { get; private set; }

        internal void SetLimit(long limit)
        {
            if (limit < 0)
            {
                throw new InvalidOperationException(Resources.ResultOptions_InvalidLimit);
            }

            if (limit >= 0)
            {
                Limit = limit;
            }
        }

        internal void SetOffset(long offset)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(Resources.ResultOptions_InvalidOffset);
            }

            if (offset >= 0)
            {
                Offset = offset;
            }
        }

        internal void SetOrdering(string by, OrderDirection direction)
        {
            if (!by.HasValue())
            {
                throw new InvalidOperationException(Resources.ResultOptions_InvalidOrderBy);
            }

            Order = new Ordering(by, direction);
        }
    }
}