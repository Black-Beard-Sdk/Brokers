using Bb.Brokers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Black.Beard.Brokers.UnitTests
{

    [TestClass]
    public class InitializerConnectionStringUnitTest
    {



        [TestMethod]
        public void TestInitializer1()
        {

            IFactoryBroker brokers = new RabbitFactoryBrokers()
            .AddServerFromConnectionString("Name=serverLocal;Hostname=localhost;Port=5672;QosPrefetchCount=1;ConnectionTimeoutSeconds=50;ConnectionRetryIntervalSeconds=50;UserName=guest;Password=guest;MaxReplayCount=50;UseLogger=true;ManagementPort=15672;ConfigAllowed=true")

            .AddPublisherFromConnectionString("Name=publisherLog;ServerName=serverLocal;DeliveryMode=Persistent;DefaultRountingKey=logTechnical;ExchangeName=ExchangeLog;ExchangeType=DIRECT")
            .AddPublisherFromConnectionString("Name=AcknowledgeQueue;ServerName=serverLocal;DeliveryMode=Persistent;DefaultRountingKey=AcknowledgeQueue;ExchangeName=AcknowledgeAction;ExchangeType=DIRECT")
            .AddPublisherFromConnectionString("Name=DeadQueue;ServerName=serverLocal;DeliveryMode=Persistent;DefaultRountingKey=DeadQueue;ExchangeName=DeadAction;ExchangeType=DIRECT")
            .AddPublisherFromConnectionString("Name=Parent;ServerName=serverLocal;DeliveryMode=Persistent;DefaultRountingKey=ParentQueue;ExchangeName=ParentAction;ExchangeType=DIRECT")
               
            .AddSubscriberFromConnectionString("Name=subscriber1;ServerName=serverLocal;StorageQueueName=queue1;Durable=true;MaxParallelism=20;ExchangeName=ExchangeName1;ExchangeType=DIRECT")
            .AddSubscriberFromConnectionString("Name=subscriberAction;ServerName=serverLocal;StorageQueueName=subscriberActionQueue;Durable=true;MaxParallelism=20;ExchangeName=subscriberActionExchangeName;ExchangeType=DIRECT")

            .Initialize(out bool result);

            Assert.AreEqual(result, true);



        }


        [TestMethod]
        public void TestInitializer()
        {

            IFactoryBroker brokers = new RabbitFactoryBrokers()
            .AddServerFromConnectionString("Name=server1;Hostname=localhost;UserName=guest;Password=guest;Port=5672;UseLogger=true")
            .AddPublisherFromConnectionString("Name=publisher1;ServerName=server1;ExchangeType=DIRECT;ExchangeName=ech1;DefaultRountingKey=ech2")
            .AddSubscriberFromConnectionString("Name=subscriber37;ServerName = server1;ExchangeType=DIRECT;ExchangeName=ech1;StorageQueueName=ech2")
            .Initialize(out bool result);

            Assert.AreEqual(result, true);

            using (var subs = new SubscriptionInstances(brokers))
            {

                Task callback(IBrokerContext ctx)
                {
                    ctx.Commit();
                    //ctx.Reject();
                    return Task.CompletedTask;
                }

                // Add a subscriber
                subs.AddSubscription("sub1", "subscriber37", callback);

                // push message in transaction
                var publisher = brokers.CreatePublisher("publisher1");
                using (publisher.BeginTransaction())
                {
                    publisher.Publish(new { uui = Guid.NewGuid() });
                    publisher.Publish(new { uui = Guid.NewGuid() });
                    publisher.Commit();
                }

                DateTime d = DateTime.Now.AddMinutes(10);
                while (DateTime.Now < d)
                    Thread.Yield();

            }

        }
    }
}
