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
