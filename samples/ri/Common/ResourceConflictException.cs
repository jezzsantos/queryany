using System;
using System.Runtime.Serialization;
using Common.Properties;

namespace Common
{
    [Serializable]
    public class ResourceConflictException : Exception
    {
        public ResourceConflictException()
            : base(Resources.ResourceConflictException_Message)
        {
        }

        public ResourceConflictException(string message)
            : base(message)
        {
        }

        public ResourceConflictException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ResourceConflictException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}