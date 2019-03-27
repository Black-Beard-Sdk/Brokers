using Bb.Configurations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bb.Brokers
{
    public interface IRabbitMessage
    {

        BasicDeliverEventArgs Message { get; set; }

        IModel Session { get; set; }

        IBroker Broker { get; set; }

        IFactoryBroker Factory { get; set; }

        BrokerSubscriptionParameter Parameters { get; set; }

    }


}
