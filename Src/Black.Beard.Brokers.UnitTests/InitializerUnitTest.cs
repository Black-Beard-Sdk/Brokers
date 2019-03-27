using Bb.Brokers;
using Bb.Configurations;
using Bb.Sdk.Brokers.Configurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Black.Beard.Brokers.UnitTests
{
    [TestClass]
    public class InitializerUnitTest
    {


        [TestMethod]
        public void TestInitializer()
        {

            IFactoryBroker brokers = new RabbitFactoryBrokers()
                .Add(
                new ServerBrokerConfiguration()
                {
                    Name = "server1",
                    Hostname = "localhost",
                    UserName = "guest",
                    Password = "guest",
                    Port = 5672,
                    UseLogger = true,
                })
                .Add(
                new BrokerPublishParameter()
                {
                    Name = "publisher",
                    ServerName = "server1",
                    ExchangeType = ExchangeType.DIRECT,
                    ExchangeName = "ech1",
                    DefaultRountingKey = "ech2"
                })
                .Add(
                new BrokerSubscriptionParameter()
                {
                    Name = "subscriber",
                    ServerName = "server1",
                    ExchangeType = ExchangeType.DIRECT,
                    ExchangeName = "ech1",
                    StorageQueueName = "ech2",
                }.AddRoutingKeys("ech2")
                );


            brokers.Initialize();

        }

        [TestMethod]
        public void TestInitializerDirectShortcut()
        {

            IFactoryBroker brokers = new RabbitFactoryBrokers()
            .Add(
                new ServerBrokerConfiguration()
                {
                    Name = "server1",
                    Hostname = "localhost",
                    UserName = "guest",
                    Password = "guest",
                    Port = 5672,
                    UseLogger = true,
                }
                )
            .AddDirect( "server1", "publisher1", "subscriber1",  "echange1", "queueDirect")
            .Initialize();
          
        }

    }
}
