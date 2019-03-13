using Bb.Configurations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            BrokerPublishParameter publisher = brokers.GetConfigurationPublisher(publisherName) as BrokerPublishParameter;
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

            BrokerSubscriptionParameter subscriber = brokers.GetConfigurationSubscriber(publisherName) as BrokerSubscriptionParameter;
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


        public static IFactoryBroker Initialize(this IFactoryBroker brokers)
        {

            foreach (string serverName in brokers.GetServerBrokerNames())
            {

                var broker = brokers.CreateServerBroker(serverName);

                Trace.WriteLine($"test to connect to {serverName}");
                if (!broker.CheckConnection())
                {

                }

            }

            foreach (string subscriberName in brokers.GetSubscriberNames())
            {
                Trace.WriteLine($"Check to subscribe {subscriberName}");
                var e = brokers.CheckSubscription(subscriberName);
                if (e != null)
                    throw e;
            }

            foreach (string publisherName in brokers.GetPublisherNames())
            {
                Trace.WriteLine($"Check publisher {publisherName}");

                var e = brokers.CheckPublisher(publisherName);
                if (e != null)
                    throw e;
            }

            return brokers;

        }

        public static IFactoryBroker Test(this IFactoryBroker brokers, out bool successfullInitialized, int timeOutInSecond = 10)
        {

            HashSet<string> cache = new HashSet<string>();

            using (var subs = new SubscriptionInstances(brokers))
            {

                Task callback(IBrokerContext ctx)
                {

                    string key = ctx.Utf8Data;

                    Trace.WriteLine($"Depiled message {key}");

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
                {
                    Trace.WriteLine($"test to subsribe {subscriberName}");
                    subs.AddSubscription(subscriberName, subscriberName, callback);
                }

                foreach (string publisherName in brokers.GetPublisherNames())
                    using (var broker = brokers.CreatePublisher(publisherName))
                    {
                        Trace.WriteLine($"test to push test message to {publisherName}");
                        var currentGuid = Guid.NewGuid().ToString();
                        cache.Add(currentGuid);
                        broker.Publish(currentGuid);
                    }

                DateTime d = DateTime.Now.AddSeconds(timeOutInSecond);
                while (cache.Count > 0)
                {

                    Thread.Yield();

                    if (d < DateTime.Now)
                        break;

                }
            }

            successfullInitialized = cache.Count == 0;

            return brokers;

        }

    }

}
