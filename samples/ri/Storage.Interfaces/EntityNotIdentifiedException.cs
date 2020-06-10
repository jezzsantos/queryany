using System;
using System.Runtime.Serialization;

namespace Storage.Interfaces
{
    [Serializable]
    public class EntityNotIdentifiedException : Exception
    {
        public EntityNotIdentifiedException()
        {
        }

        public EntityNotIdentifiedException(string message) : base(message)
        {
        }

        public EntityNotIdentifiedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntityNotIdentifiedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}