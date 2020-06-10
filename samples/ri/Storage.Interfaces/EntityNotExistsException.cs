using System;
using System.Runtime.Serialization;

namespace Storage.Interfaces
{
    [Serializable]
    public class EntityNotExistsException : Exception
    {
        public EntityNotExistsException()
        {
        }

        public EntityNotExistsException(string message) : base(message)
        {
        }

        public EntityNotExistsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntityNotExistsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}