using System;
using System.Runtime.Serialization;
using Common.Properties;

namespace Common
{
    [Serializable]
    public class MethodNotAllowedException : Exception
    {
        public MethodNotAllowedException()
            : base(Resources.MethodNotAllowedException_Message)
        {
        }

        public MethodNotAllowedException(string message)
            : base(message)
        {
        }

        public MethodNotAllowedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected MethodNotAllowedException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}