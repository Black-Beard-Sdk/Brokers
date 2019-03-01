using System.Collections.Generic;

namespace Bb.Brokers
{

    /// <summary>
    /// Representation of a message coming from a message broker.
    /// Low-level interface - most of the time BasicMessage will be used instead inside modules.
    /// </summary>
    public interface IBrokerContext
    {

        /// <summary>
        /// Return identifier from the message
        /// </summary>
        object TransactionId { get; }

        /// <summary>
        /// Return the message from utf8. 
        /// </summary>s
        string Utf8Data { get; }

        /// <summary>
        /// The routing key used when the message was originally published.
        /// </summary>
        string RoutingKey { get; }

        /// <summary>
        /// The exchange the message was originally published to
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// A message may have headers. (can be null or empty).
        /// </summary>
        IDictionary<string, object> Headers { get; set; }

        /// <summary>
        /// Latest message read is marked as correctly read and should never be presented again (may actually happen).
        /// </summary>
        void Commit();

        /// <summary>
        /// Discard a message, never present it again.
        /// </summary>
        void Reject();

        /// <summary>
        /// return true if the current message can be pushed at last of queue
        /// </summary>
        /// <returns></returns>
        bool CanBeRequeued();

        /// <summary>
        /// Will put the message back in the queue, at the start of the queue.
        /// </summary>
        void RequeueLast();

        /// <summary>
        /// Discard a message, represent it later.
        /// </summary>
        void Rollback();

    }
}
