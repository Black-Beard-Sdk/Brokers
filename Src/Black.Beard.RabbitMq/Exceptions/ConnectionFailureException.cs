using System;

namespace Bb.Exceptions
{
    [System.Serializable]
    public class ConnectionFailureException : BrokerException
    {
        public ConnectionFailureException() : base() { }

        public ConnectionFailureException(string message) : base(message) { }
        public ConnectionFailureException(string message, Exception e) : base(message, e) { }
    }
}
