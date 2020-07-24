using System;
using QueryAny.Primitives;
using QueryAny.Properties;

namespace QueryAny
{
    public class ResultOptions
    {
        public const int DefaultLimit = -1;
        public const int DefaultOffset = 0;
        public const OrderDirection DefaultOrderDirection = OrderDirection.Ascending;
        public static readonly string DefaultOrder = null;

        public ResultOptions()
        {
            Limit = DefaultLimit;
            Offset = DefaultOffset;
            OrderBy = new Ordering(DefaultOrder, DefaultOrderDirection);
        }

        public int Limit { get; private set; }
        public int Offset { get; private set; }
        public Ordering OrderBy { get; private set; }

        internal void SetLimit(int limit)
        {
            if (limit < 0)
            {
                throw new InvalidOperationException(Resources.ResultOptions_InvalidLimit);
            }

            Limit = limit;
        }

        internal void SetOffset(int offset)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(Resources.ResultOptions_InvalidOffset);
            }

            Offset = offset;
        }

        internal void SetOrdering(string by, OrderDirection direction)
        {
            if (!by.HasValue())
            {
                throw new InvalidOperationException(Resources.ResultOptions_InvalidOrderBy);
            }

            OrderBy = new Ordering(by, direction);
        }
    }
}
