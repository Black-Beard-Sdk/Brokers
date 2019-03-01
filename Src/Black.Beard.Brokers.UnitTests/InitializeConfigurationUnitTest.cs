using Bb.LogAppender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Black.Beard.Brokers.UnitTests
{
    [TestClass]
    public class InitializeConfigurationUnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {

            var brokers = new Bb.Brokers.RabbitFactoryBrokers()
            .Add(new Bb.Configurations.ServerBrokerConfiguration() { Name = "Server1" })
            .Add(new Bb.Configurations.BrokerPublishParameter() { Name = "publisher1", ServerName = "Server1" })
                ;

            var log = RabbitMqAppender.Initialize(brokers, "CustomTraceListener", "publisher1", true, 2);

            Trace.WriteLine("Test");


        }


    }
}
