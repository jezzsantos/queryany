using System;

namespace Storage.Interfaces
{

    [Serializable]
    public class EntityNotExistsException : Exception
    {
        public EntityNotExistsException() { }
        public EntityNotExistsException(string message) : base(message) { }
        public EntityNotExistsException(string message, Exception inner) : base(message, inner) { }
        protected EntityNotExistsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
