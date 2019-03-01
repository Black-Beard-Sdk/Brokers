using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bb.Configurations
{
    public abstract class BrokerParameter
    {

        public string Name { get; set; }      

        /// <summary>
        /// Exchange to bind to. Empty/null for default exchange.
        /// </summary>
        [Description("Exchange to bind to. Empty/null for default exchange.")]
        public string ExchangeName { get; set; } = null;

        /// <summary>
        /// Exchange type. Used to create the exchange if it does not exist and to interpret the routing keys.
        /// </summary>
        [Description("Exchange type. Used to create the exchange if it does not exist and to interpret the routing keys.")]
        public ExchangeType ExchangeType { get; set; } = ExchangeType.DIRECT;

        /// <summary>
        /// Server name configuration.
        /// </summary>
        [Description("Server name configuration.")]
        [Required]
        public string ServerName { get; set; }

    }
}
