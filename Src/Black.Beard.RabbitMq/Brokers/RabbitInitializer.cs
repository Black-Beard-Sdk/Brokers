using Bb.Configurations;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    public static class RabbitInitializer
    {

        public static IFactoryBroker AddDirect(this IFactoryBroker brokers, string serverName
            , string publisherName
            , string subscriberName
            , string exchangeName
            , string queueName
            )
        {

            var e = brokers.CheckServerBroker(serverName);
            if (e != null)
                throw e;

            BrokerPublishParameter publisher = brokers.GetPublisher(publisherName) as BrokerPublishParameter;
            if (publisher == null)
            {
                brokers.Add(publisher = new BrokerPublishParameter()
                {
                    Name = publisherName,
                    ServerName = serverName,
                    ExchangeType = ExchangeType.DIRECT,
                    ExchangeName = exchangeName,
                    DefaultRountingKey = queueName
                });
            }
            else
            {

            }

            BrokerSubscriptionParameter subscriber = brokers.GetSubscriber(publisherName) as BrokerSubscriptionParameter;
            if (subscriber == null)
            {
                brokers.Add(subscriber = new BrokerSubscriptionParameter()
                {
                    Name = subscriberName,
                    ServerName = serverName,
                    ExchangeType = ExchangeType.DIRECT,
                    ExchangeName = exchangeName,
                    StorageQueueName = queueName,                    
                });
            }

            return brokers;

        }


        public static IFactoryBroker Initialize(this IFactoryBroker brokers)
        {

            foreach (string serverName in brokers.GetServerBrokerNames())
            {

                var broker = brokers.CreateServerBroker(serverName);

                if (!broker.CheckConnection())
                {

                }

            }

            foreach (string publisherName in brokers.GetPublisherNames())
            {

                var broker = brokers.CreatePublisher(publisherName);

                broker.Publish("test");

            }
            Task callback(IBrokerContext broker)
            {
                return Task.CompletedTask;
            }

            foreach (string subscriberName in brokers.GetSubscriberNames())
            {

                var broker = brokers.CreateSubscription(subscriberName, callback);

            }

            return brokers;

        }

    }

}
