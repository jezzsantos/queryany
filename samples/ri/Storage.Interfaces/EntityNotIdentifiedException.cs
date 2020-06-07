using System;

namespace Storage.Interfaces
{

    [Serializable]
    public class EntityNotIdentifiedException : Exception
    {
        public EntityNotIdentifiedException() { }
        public EntityNotIdentifiedException(string message) : base(message) { }
        public EntityNotIdentifiedException(string message, Exception inner) : base(message, inner) { }
        protected EntityNotIdentifiedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
