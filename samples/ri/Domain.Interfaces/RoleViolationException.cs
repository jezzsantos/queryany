using System;
using System.Runtime.Serialization;
using Domain.Interfaces.Properties;

namespace Domain.Interfaces
{
    /// <summary>
    ///     Defines the <see cref="RoleViolationException" /> exception.
    /// </summary>
    [Serializable]
    public class RoleViolationException : Exception
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RoleViolationException" /> class.
        /// </summary>
        public RoleViolationException()
            : base(Resources.RoleViolationException_Message)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="RoleViolationException" /> class.
        /// </summary>
        /// <paramref name="message"> The message of the exception </paramref>
        public RoleViolationException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="RoleViolationException" /> class.
        /// </summary>
        /// <paramref name="message"> The message of the exception </paramref>
        /// <paramref name="inner"> The inner exception </paramref>
        public RoleViolationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="RoleViolationException" /> class.
        /// </summary>
        /// <paramref name="info"> The serialization information </paramref>
        /// <paramref name="context"> The context for the exception </paramref>
        protected RoleViolationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}