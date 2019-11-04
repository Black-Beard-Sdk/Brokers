using System;

namespace Bb.Brokers
{

    /// <summary>
    /// Represents a subscription to a given queue inside the message broker.
    /// </summary>
    public interface IBrokerSubscription : IDisposable
    {

        /// <summary>
        /// Gets the broker that create current subscription
        /// </summary>
        /// <value>
        /// The broker.
        /// </value>
        IBroker Broker { get; }

        void Close();

    }
}
