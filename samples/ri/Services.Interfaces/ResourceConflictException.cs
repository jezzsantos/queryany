﻿using System;
using System.Runtime.Serialization;

namespace Services.Interfaces
{
    [Serializable]
    public class ResourceConflictException : Exception
    {
        public ResourceConflictException()
            : base("The resource already exists, or is in a conflicted state.")
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