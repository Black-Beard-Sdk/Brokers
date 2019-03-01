using Bb.Brokers;
using Bb.Configurations;
using Bb.Sdk.Brokers.Configurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Black.Beard.Brokers.UnitTests
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestSerializeDeserializeJSon()
        {

            string server = new ServerBrokerConfiguration()
            {
                Name = "n1",
                Hostname = "localhost",
                UserName = "Guest",
                Password = "Guest",
                Port = 15672,
                UseLogger = true,
            }.SaveToJSon();

            var server2 = server.LoadServerFromJson();

            Assert.AreEqual(server2.Hostname, "localhost");

        }

        [TestMethod]
        public void TestReplaceEnvironmentJSon()
        {

            Environment.SetEnvironmentVariable("host", "test1");
            Environment.SetEnvironmentVariable("port", "15672");
            Environment.SetEnvironmentVariable("connectionTimeoutSeconds", "145");

            string server = @"
                {
                    ""Name"":""n1"",
                    ""Hostname"":""@host"",
                    ""Port"":""@port"",
                    ""UserName"":""Guest"",
                    ""Password"":""Guest"",
                    ""ConnectionTimeoutSeconds"": ""@connectionTimeoutSeconds"",
                    ""ConnectionRetryIntervalSeconds"":5,
                    ""MaxReplayCount"":20,
                    ""UseLogger"":true,
                    ""ManagementPort"":15672,
                    ""ConfigAllowed"":true
                }";

            var server2 = server.LoadServerFromJson();

            Assert.AreEqual(server2.Hostname, "test1");
            Assert.AreEqual(server2.Port, 15672);
            Assert.AreEqual(server2.ConnectionTimeoutSeconds, 145);

        }

        [TestMethod]
        public void TestDeserialize()
        {

            var server = @"Name=n1;Hostname=localhost;UserName=Guest;Password=Guest;Port = 15672;UseLogger = true;ConnectionTimeoutSeconds=145".LoadServerLoadFromConnectionString();

            Assert.AreEqual(server.Name, "n1");
            Assert.AreEqual(server.Hostname, "localhost");
            Assert.AreEqual(server.Port, 15672);
            Assert.AreEqual(server.ConnectionTimeoutSeconds, 145);

        }

        [TestMethod]
        public void TestDeserializeWithEnvironment()
        {

            Environment.SetEnvironmentVariable("host", "test1");
            Environment.SetEnvironmentVariable("port", "15672");
            Environment.SetEnvironmentVariable("connectionTimeoutSeconds", "145");

            var server = @"Name=n1;Hostname=@host;UserName=Guest;Password=Guest;Port = @port;UseLogger = true;ConnectionTimeoutSeconds=@connectionTimeoutSeconds".LoadServerLoadFromConnectionString();

            Assert.AreEqual(server.Name, "n1");
            Assert.AreEqual(server.Hostname, "test1");
            Assert.AreEqual(server.Port, 15672);
            Assert.AreEqual(server.ConnectionTimeoutSeconds, 145);


        }

    }
}
