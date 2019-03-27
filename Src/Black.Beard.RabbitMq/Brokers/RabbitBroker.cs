using Bb.Configurations;
using Bb.Exceptions;
using Bb.Helpers;
using EasyNetQ.Management.Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    /// <summary>
    /// Main entry point for all RabbitMQ operations
    /// </summary>
    public sealed class RabbitBroker : IBroker
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitBroker"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="InvalidOperationException">the broker is used without configuration</exception>
        public RabbitBroker(ServerBrokerConfiguration configuration)
        {

            Configuration = configuration;

            if (Configuration == null)
            {
                Trace.WriteLine("the broker is used without configuration", TraceLevel.Error.ToString());
                throw new InvalidOperationException("the broker is used without configuration");
            }
        }

        internal IModel GetChannel()
        {
            Init();
            return _connection.CreateModel();
        }

        private Uri GetEndpoint()
        {

            // Sessions (i.e. channels, i.e. IModel) are created from the connexion, but are not thread safe.
            var endPoint = Configuration.Hostname.LocalhostHandling();

            Uri uri;
            if (string.IsNullOrEmpty(Configuration.VirtualHost))
                uri = new Uri($"amqp://{endPoint}:{Configuration.Port}");
            else
                uri = new Uri($"amqp://{endPoint}:{Configuration.Port}/{Configuration.VirtualHost}");

            return uri;

        }

        private void Init()
        {

            if (_connection == null)
                lock (_lock)
                    if (_connection == null)    // Create the connection
                    {

                        _connection = CreateConnection();

                        try
                        {
                            using (var channel = GetChannel()) { }    // Test connection
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(new { Message = e.Message, Exception = e });
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();

                            throw;
                        }

                    }

        }

        /// <summary>
        /// Checks the broker server connection.
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection()
        {

            try
            {
                var _cnx = CreateConnection();
                using (var channel = _cnx.CreateModel()) { }    // Test connection
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(new { e.Message, Exception = e });
                return false;
            }

        }

        private IConnection CreateConnection()
        {
            var rabbitMqFactory = new ConnectionFactory()
            {
                Uri = GetEndpoint(),
                UserName = Configuration.UserName,
                Password = Configuration.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                RequestedHeartbeat = 5,
                RequestedConnectionTimeout = 20000,
                ContinuationTimeout = TimeSpan.FromSeconds(60),
            };

            return CreateConnectionWithTimeout(rabbitMqFactory);
        }

        private ManagementClient Manager()
        {

            if (Configuration.ConfigAllowed && Configuration.ManagementPort.HasValue)
            {
                try
                {
                    _managmentClient = new ManagementClient(_connection.Endpoint.HostName, Configuration.UserName, Configuration.Password, Configuration.ManagementPort.Value);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(new { Message = "Failed to create _managment client", Exception = e }, TraceLevel.Error.ToString());
                    throw;
                }

            }

            return _managmentClient;

        }

        /// <summary>
        /// Remove all data from broker.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IllegalStateException">cannot purge RabbitMQ broker without a management connection</exception>
        public async Task Reset()
        {

            Init();

            var manager = Manager();

            if (manager == null)
                throw new IllegalStateException("cannot purge RabbitMQ broker without a management connection");

            await Task.WhenAll((await manager.GetQueuesAsync()).Select(q => manager.PurgeAsync(q)));
            await Task.WhenAll((await manager.GetBindingsAsync()).Where(q => q.DestinationType == "queue" && !string.IsNullOrEmpty(q.RoutingKey) && !string.IsNullOrEmpty(q.Source)).Select(q => manager.DeleteBindingAsync(q)));

        }

        private IConnection CreateConnectionWithTimeout(ConnectionFactory rabbitMqFactory)
        {
            IConnection connection = null;
            if (Configuration.UseLogger)
                Trace.WriteLine($"Attempting to connect to RabbitMQ with connectionString: {rabbitMqFactory.Uri}", TraceLevel.Info.ToString());

            if (Configuration.ConnectionTimeoutSeconds > 0)
            {
                var limit = DateTime.Now.AddSeconds(Configuration.ConnectionTimeoutSeconds);
                while (connection == null && DateTime.Now <= limit)
                {
                    try
                    {
                        connection = rabbitMqFactory.CreateConnection();
                    }
                    catch (BrokerUnreachableException e)
                    {
                        if (e.InnerException != null)
                            if (e.InnerException is RabbitMQ.Client.Exceptions.AuthenticationFailureException)
                            {
                                Trace.WriteLine(new { Message = e.InnerException.Message, Exception = e }, TraceLevel.Error.ToString());
                                throw new AuthenticationException(e.InnerException.Message);
                            }


                        if (Configuration.UseLogger)
                            Trace.WriteLine(new { Message = $"Could not connect to RabbitMQ broker. Will retry in {Configuration.ConnectionRetryIntervalSeconds} seconds.", Exception = e }, TraceLevel.Error.ToString());

                        // sniff
                        Thread.Sleep(1000 * Configuration.ConnectionRetryIntervalSeconds);

                    }
                }

                if (connection == null)
                {
                    if (Configuration.UseLogger)
                        Trace.WriteLine("Giving up on opening a connection to the RabbitMQ broker", TraceLevel.Error.ToString());
                    else
                        Console.WriteLine("Giving up on opening a connection to the RabbitMQ broker");

                    throw new ConnectionFailureException($"Cannot reach broker on {GetEndpoint().ToString()}");
                }

                if (Configuration.UseLogger)
                    Trace.WriteLine("Successfully connected to RabbitMQ", TraceLevel.Info.ToString());

                return connection;
            }
            else
            {
                return rabbitMqFactory.CreateConnection();
            }
        }

        /// <summary>
        /// Register a new subscription to an existing queue (i.e. on the default exchange)
        /// </summary>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <param name="callback">The callback that contains business code.</param>
        /// <param name="factory">The factory is optional if you want override context. by default the value is () =&gt; new <see cref="!:Bb.Brokers.RabbitBrokerContext" />()</param>
        /// <returns></returns>
        /// <exception cref="InvalidConfigurationException">subscriptionParameters must be of type BrokerSubscriptionParameters</exception>
        public IBrokerSubscription Subscribe(object subscriptionParameters, Func<IBrokerContext, Task> callback, Func<IBrokerContext> factory = null)
        {

            if (factory == null)
                factory = () => new RabbitBrokerContext();

            BrokerSubscriptionParameter _subscriptionParameters = (subscriptionParameters as BrokerSubscriptionParameter) ?? throw new InvalidConfigurationException("subscriptionParameters must be of type BrokerSubscriptionParameters");

            var res = new RabbitBrokerSubscription();
            res.Subscribe(this, _subscriptionParameters, callback, factory);
            return res;
        }

        /// <summary>
        /// Get a new instance of a publisher on an exchange.
        /// </summary>
        /// <param name="brokerPublishParameters"></param>
        /// <returns>
        /// A ready to publish publisher
        /// </returns>
        /// <exception cref="InvalidConfigurationException">brokerPublishParameters must be of type BrokerPublishParameters</exception>
        public IBrokerPublisher GetPublisher(object brokerPublishParameters)
        {

            BrokerPublishParameter _brokerPublishParameters = brokerPublishParameters as BrokerPublishParameter
                ?? throw new InvalidConfigurationException("brokerPublishParameters must be of type BrokerPublishParameters");

            return new RabbitBrokerPublisher(this, _brokerPublishParameters);

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
                if (_connection != null)
                {
                    _connection?.Close();
                    _connection = null;
                }
        }

        /// <summary>
        /// Message count in queue in internal broker. 0 if queue does not exist.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public Task<int> GetQueueDepth(string queueName)
        {
            using (var channel = GetChannel())
            {
                try
                {
                    return Task.FromResult((int)channel.MessageCount(queueName));
                }
                catch (Exception)
                {
                    return Task.FromResult(0);
                }
            }
        }


        /// <summary>
        /// Global RabbitMQ configuration (from config file).
        /// </summary>
        public ServerBrokerConfiguration Configuration { get; private set; }

        public IFactoryBroker Factory { get; internal set; }

        ///// <summary>
        ///// If true, all queues will be purged on startup. Only used in tests.
        ///// </summary>
        //public bool PurgeBrokerOnStartup { get; set; } = false;

        ///// <summary>
        ///// Declare a simple queue bound on the default exchange with routing key = queue name.
        ///// </summary>
        ///// <param name="queueName"></param>
        //public void QueueDeclare(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
        //{
        //    using (var channel = GetChannel())
        //    {
        //        channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);
        //    }
        //}

        //public void BindTopic(string queueName, string exchangeName)
        //{
        //    using (var channel = GetChannel())
        //    {
        //        channel.QueueDeclare(queueName, true, false, false, null);
        //        channel.ExchangeDeclare(exchangeName, "topic", true, false, null);
        //        channel.QueueBind(queueName, exchangeName, "*", null);
        //    }
        //}

        private IConnection _connection;
        private ManagementClient _managmentClient;
        private readonly object _lock = new object();

    }

}
