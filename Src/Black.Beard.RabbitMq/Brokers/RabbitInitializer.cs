using Bb.Configurations;
using System;
using System.Collections.Generic;
using System.Threading;
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
                }.AddRoutingKeys(queueName)

                );
            }

            return brokers;

        }


        public static IFactoryBroker Initialize(this IFactoryBroker brokers, out bool successfullInitialized)
        {

            foreach (string serverName in brokers.GetServerBrokerNames())
            {

                var broker = brokers.CreateServerBroker(serverName);

                if (!broker.CheckConnection())
                {

                }

            }

                HashSet<string> cache = new HashSet<string>();

            using (var subs = new SubscriptionInstances(brokers))
            {


                Task callback(IBrokerContext ctx)
                {
                    string key = ctx.Utf8Data;
                    if (cache.Contains(key))
                    {
                        cache.Remove(ctx.Utf8Data);
                        ctx.Commit();
                    }
                    else
                        ctx.Reject();

                    return Task.CompletedTask;

                }

                foreach (string subscriberName in brokers.GetSubscriberNames())
                    subs.AddSubscription(subscriberName, subscriberName, callback);

                foreach (string publisherName in brokers.GetPublisherNames())
                    using (var broker = brokers.CreatePublisher(publisherName))
                    {
                        var currentGuid = Guid.NewGuid().ToString();
                        cache.Add(currentGuid);
                        broker.Publish(currentGuid);
                    }

                DateTime d = DateTime.Now.AddSeconds(10);
                while (DateTime.Now < d || cache.Count != 0)
                    Thread.Yield();

            }

            successfullInitialized = cache.Count == 0;

            return brokers;

        }

    }

}
