using System;
using System.Runtime.Serialization;

namespace Thirdweb.EWS
{
    [Serializable]
    internal class VerificationException : Exception
    {
        internal bool CanRetry { get; }

        public VerificationException(string message, bool canRetry)
            : base(message)
        {
            CanRetry = canRetry;
        }

        protected VerificationException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
