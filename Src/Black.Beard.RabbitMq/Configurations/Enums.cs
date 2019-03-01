namespace Bb.Configurations
{

    /// <summary>
    /// Delivery mode of a message
    /// </summary>
    public enum DeliveryMode : byte
    {
        NonPersistent = 1,
        Persistent = 2
    }

    /// <summary>
    /// The different AMQP exchange types we allow.
    /// </summary>
    public enum ExchangeType
    {
        DIRECT,
        TOPIC,
        /// <summary>
        /// DO NOT USE - for compatibility with shared rabbitmq library only.
        /// </summary>
        FANOUT
    }
 
}
