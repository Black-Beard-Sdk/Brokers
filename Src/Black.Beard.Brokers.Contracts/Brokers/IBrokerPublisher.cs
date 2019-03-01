using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    /// <summary>
    /// Base interface for publishing messages on AMQP exchanges.
    /// </summary>
    public interface IBrokerPublisher : IDisposable
    {

        ITransaction BeginTransaction();

        void Commit();

        void Rollback();

        /// <summary>
        /// Publish a message on a broker exchange.
        /// </summary>
        /// <param name="routingKey">routing key</param>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        Task Publish(string routingKey, object message, object headers = null);

        /// <summary>
        /// Publish a message on a broker exchange with default routing key.
        /// </summary>
        /// <param name="routingKey">routing key</param>
        /// <param name="message"></param>
        /// <param name="headers"></param>
        Task Publish(object message, object headers = null);
    }
}
