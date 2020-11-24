using System;
using System.Runtime.Serialization;
using Domain.Interfaces.Properties;

namespace Domain.Interfaces
{
    [Serializable]
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base(Resources.ForbiddenException_Message)
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