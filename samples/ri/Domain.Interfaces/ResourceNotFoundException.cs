using System;
using System.Runtime.Serialization;
using Domain.Interfaces.Properties;

namespace Domain.Interfaces
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