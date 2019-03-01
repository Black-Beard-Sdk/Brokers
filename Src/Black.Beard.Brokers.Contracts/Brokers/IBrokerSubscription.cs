using System;

namespace Bb.Brokers
{

    /// <summary>
    /// Represents a subscription to a given queue inside the message broker.
    /// </summary>
    public interface IBrokerSubscription : IDisposable
    {
    }
}
