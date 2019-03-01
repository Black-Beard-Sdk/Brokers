using System.Collections.Generic;
using System.ComponentModel;

namespace Bb.Configurations
{

    /// <summary>
    /// Describes what basically amounts to a persistent AMQP subscription
    /// </summary>
    public class BrokerSubscriptionParameter : BrokerParameter
    {

        public BrokerSubscriptionParameter()
        {

        }

        //public BrokerSubscriptionParameters(string connectionString)
        //{
        //    if (!ConnectionStringHelper.Map(this, connectionString))
        //    {

        //    }
        //}

        /// <summary>
        /// The routing keys to accept (the "binding keys"). Leave empty for all. Ignored for default exchange.
        /// </summary>
        [Description("The routing keys to accept (the \"binding keys\"). Leave empty for all. Ignored for default exchange.")]
        public List<string> AcceptedRoutingKeys { get; set; } = new List<string>(10);

        public BrokerSubscriptionParameter AddRoutingKeys(params string[] rountingKeys)
        {
            AcceptedRoutingKeys.AddRange(rountingKeys);
            return this;
        }

        /// <summary>
        /// The subscription yields messages which are stored in a queue before they can be consumed by the client message processor.
        /// </summary>
        [Description("The subscription yields messages which are stored in a queue before they can be consumed by the client message processor.")]
        public string StorageQueueName { get; set; }

        /// <summary>
        /// Subscription stays on even if the consumers are dead. By default the value is true
        /// </summary>
        [Description("True: subscription stays on even if the consumers are dead. By default the value is true")]
        public bool Durable { get; set; } = true;

        /// <summary>
        /// For a single process (there may be many processes!) never process more than MaxParallelism messages concurrently. By default the value is 2
        /// </summary>
        [Description("For a single process (there may be many processes!) never process more than MaxParallelism messages concurrently. By default the value is 2")]
        public int MaxParallelism { get; set; } = 2;

        /// <summary>
        /// time duration for waiting all process finish before close dirty. By default the value is 10
        /// </summary>
        [Description("time duration for waiting all process finish before close dirty. By default the value is 10")]
        public int MaxTimeWaitingToClose { get; set; } = 10;

        /// <summary>
        /// How many times the message may be requeued before being labeled a poison message. -1 to disable. By default the value is 20
        /// </summary>
        [Description("How many times the message may be requeued before being labeled a poison message. -1 to disable. By default the value is 20")]
        public int MaxReplayCount { get; set; } = 20;

        /// <summary>
        /// The header used for the poison message mechanism. By default the value is "REPLAY"
        /// </summary>
        [Description("The header used for the poison message mechanism.")]
        public string ReplayHeaderKey { get; set; } = "REPLAY";

        /// <summary>
        /// interval in seconds for survey the subscriber and prevent network cut.
        /// </summary>
        [Description("interval in seconds for survey the subscriber and prevent network cut.")]
        public int WatchDog { get; set; } = 30;



    }
}
