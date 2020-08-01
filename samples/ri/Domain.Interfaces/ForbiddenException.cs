using System;
using System.Runtime.Serialization;

namespace Services.Interfaces
{
    [Serializable]
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base(Properties.Resources.ForbiddenException_Message)
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {
        }

        public ForbiddenException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ForbiddenException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}