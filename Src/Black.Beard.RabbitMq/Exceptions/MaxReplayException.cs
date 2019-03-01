using Bb.Brokers;
using System;

namespace Bb.Exceptions
{
    [Serializable]
    public class MaxReplayException : Exception
    {

        public MaxReplayException(int count, IBrokerContext context)
        {
            this.MaxReplay = count;
            this.Context = context;
        }

        public MaxReplayException(string message) : base(message) { }
        public MaxReplayException(string message, Exception inner) : base(message, inner) { }
        protected MaxReplayException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public int MaxReplay { get; }
        public IBrokerContext Context { get; }

    }

}