using System;

namespace Bb.Exceptions
{

    [System.Serializable]
    public class BrokerException : Exception
    {
        public BrokerException() : base() { }
        public BrokerException(string message) : base(message) { }
        public BrokerException(string message, System.Exception inner) : base(message, inner) { }
        protected BrokerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}