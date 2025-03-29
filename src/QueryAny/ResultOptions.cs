using System;
using QueryAny.Extensions;
using QueryAny.Properties;

namespace QueryAny
{
    public class ResultOptions
    {
        public const int DefaultLimit = -1;
        public const int DefaultOffset = -1;
        public const OrderDirection DefaultOrderDirection = OrderDirection.Ascending;
        public static readonly string DefaultOrder = null;

        public int Limit { get; private set; } = DefaultLimit;

        public int Offset { get; private set; } = DefaultOffset;

        public Ordering OrderBy { get; private set; } = new(DefaultOrder, DefaultOrderDirection);

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