using System.ComponentModel;

namespace Bb.Configurations
{
    /// <summary>
    /// Describe how to publish messages on an AMQP exchange.
    /// </summary>
    public class BrokerPublishParameter : BrokerParameter
    {

        public BrokerPublishParameter()
        {

        }

        //public BrokerPublishParameters(string connectionString)
        //{
        //    if (!ConnectionStringHelper.Map(this, connectionString))
        //    {
        //    }
        //}

        /// <summary>
        /// Whether the messages published will be persistent between reboots or not.
        /// </summary>
        [Description("Whether the messages published will be persistent between reboots or not.")]
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Persistent;

        /// <summary>
        /// If no routing key is specified at publish time, use this one. Can be null.
        /// Especially useful for default exchange publishers aimed at a specific queue.
        /// </summary>
        [Description("If no RountingKey name is specified at publish time, use this one. Can be null. Especially useful for default exchange publishers aimed at a specific queue.")]
        public string DefaultRountingKey { get; set; } = null;

    }

}
