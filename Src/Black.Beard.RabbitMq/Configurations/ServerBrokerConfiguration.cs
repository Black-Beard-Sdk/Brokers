using Bb.Helpers;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bb.Configurations
{

    public class ServerBrokerConfiguration
    {

        public ServerBrokerConfiguration()
        {

        }

        /// <summary>
        /// Name of the configuration
        /// </summary>
        [Description("Name key for identification")]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Broker connection string, like amqp://lapin:5672
        /// </summary>
        [Description("Broker hostname")]
        [Required]
        public string Hostname { get; set; }
        
        [Description("Broker connection port.")]
        public int Port { get; set; }

        /// <summary>
        /// RabbitMQ login.
        /// </summary>
        [Description("RabbitMQ login.")]
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// RabbitMQ password
        /// </summary>
        [Description("RabbitMQ password")]
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Should make connection attempts for this long. It crashes if 
        /// no connections could be created within this timeframe.
        /// If 0, only one attempt is done.
        /// </summary>
        [Description("Should make connection attempts for this long. It crashes if no connections could be created within this timeframe. If 0, only one attempt is done.")]
        public int ConnectionTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// How often we should attempt to connect.
        /// </summary>
        [Description("How often we should attempt to connect.")]
        public int ConnectionRetryIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// The number of max replay count
        /// </summary>
        [Description("MaxReplayCount")]
        public int MaxReplayCount { get; set; } = 20;

        /// <summary>
        /// Use Logger when thrown exception
        /// </summary>
        [Description("Use Logger when thrown exception")]
        public bool UseLogger { get; set; } = true;

        /// <summary>
        /// Only needed for purging queues. Null means management is disabled. Set to null in production.
        /// </summary>
        [Description("Only needed for purging queues. Null means management is disabled. Set to null in production.")]
        public int? ManagementPort { get; set; } = 15672;

        /// <summary>
        /// Set to false if the user does not have permissions to change broker configuration
        /// </summary>
        [Description("Set to false if the user does not have permissions to change broker configuration")]
        public bool ConfigAllowed { get; set; } = true;

        ///// <summary>
        ///// Configure prefetch count for Rabbit Qos
        ///// </summary>
        //[Description("Configure prefetch count for Rabbit Qos")]
        //public ushort QosPrefetchCount { get; set; }

    }
}
