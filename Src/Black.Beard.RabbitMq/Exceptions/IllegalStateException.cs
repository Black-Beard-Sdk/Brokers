using System;

namespace Bb.Exceptions
{
    public class IllegalStateException : BrokerException
    {
        public IllegalStateException(string message) : base(message) { }
    }
}
