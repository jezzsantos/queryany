using System;
using System.Runtime.Serialization;
using Common.Properties;

namespace Common
{
    [Serializable]
    public class RuleViolationException : Exception
    {
        public RuleViolationException()
            : base(Resources.RuleViolationException_Message)
        {
        }

        public RuleViolationException(string message)
            : base(message)
        {
        }

        public RuleViolationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected RuleViolationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}