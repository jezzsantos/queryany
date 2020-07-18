using System;
using QueryAny.Properties;

namespace QueryAny
{
    public class ResultOptions
    {
        public const long DefaultLimit = -1;
        public const long DefaultOffset = 0;

        public ResultOptions()
        {
            Limit = DefaultLimit;
            Offset = DefaultOffset;
        }

        public long Limit { get; private set; }

        public long Offset { get; private set; }

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
    }
}