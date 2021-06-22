using System;
using System.Runtime.Serialization;
using Common.Properties;

namespace Common
{
    [Serializable]
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException()
            : base(Resources.ResourceNotFoundException_Message)
        {
        }

        public ResourceNotFoundException(string message)
            : base(message)
        {
        }

        public ResourceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ResourceNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}