using System;
using System.Runtime.Serialization;
using Api.Common.Properties;

namespace Api.Common
{
    internal class UnexpectedException : Exception
    {
        public UnexpectedException()
            : base(Resources.UnexpectedException_Message)
        {
        }

        public UnexpectedException(Exception inner)
            : base(Resources.UnexpectedException_Message, inner)
        {
        }

        public UnexpectedException(string message)
            : base(message)
        {
        }

        public UnexpectedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UnexpectedException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}