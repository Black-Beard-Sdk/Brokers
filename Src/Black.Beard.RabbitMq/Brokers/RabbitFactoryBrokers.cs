using Bb.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    public class RabbitFactoryBrokers : IFactoryBroker
    {

        public RabbitFactoryBrokers()
        {
            _serverConfigurations = new Dictionary<string, ServerBrokerConfiguration>();
            _brokerPublishConfigurations = new Dictionary<string, BrokerPublishParameter>();
            _brokerSubscriptionConfigurations = new Dictionary<string, BrokerSubscriptionParameter>();
        }

        #region configuration

        /// <summary>
        /// Append a new configuration
        /// </summary>
        /// <param name="configuration"></param>
        public IFactoryBroker Add(object configuration)
        {

            if (configuration is ServerBrokerConfiguration c)
            {
                if (_serverConfigurations.TryGetValue(c.Name, out ServerBrokerConfiguration result))
                    _serverConfigurations[c.Name] = c;
                else
                    _serverConfigurations.Add(c.Name, c);
            }

            else if (configuration is BrokerPublishParameter d)
            {
                if (_brokerPublishConfigurations.TryGetValue(d.Name, out BrokerPublishParameter result))
                    _brokerPublishConfigurations[d.Name] = d;
                else
                    _brokerPublishConfigurations.Add(d.Name, d);
            }

            else if (configuration is BrokerSubscriptionParameter e)
            {
                if (_brokerSubscriptionConfigurations.TryGetValue(e.Name, out BrokerSubscriptionParameter result))
                    _brokerSubscriptionConfigurations[e.Name] = e;
                else
                    _brokerSubscriptionConfigurations.Add(e.Name, e);
            }

            else
                throw new Exceptions.InvalidConfigurationException($" {nameof(configuration)} object must be of type ServerBrokerConfiguration, BrokerPublishParameters or BrokerSubscriptionParameters");

            return this;
        }

        #endregion configuration

        #region Servers

        /// <summary>
        /// Create broker server from specified configuration server name
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        public IBroker CreateServerBroker(string serverName)
        {

            if (!_serverConfigurations.TryGetValue(serverName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {serverName}");

            var _broker = new RabbitBroker(server);

            return _broker;

        }

        /// <summary>
        /// Create broker server from specified configuration server name
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        public Exception CheckServerBroker(string serverName)
        {

            if (!_serverConfigurations.TryGetValue(serverName, out ServerBrokerConfiguration server))
                return new Exceptions.InvalidConfigurationException($"configuration server {serverName}");

            return null;

        }

        /// <summary>
        /// Gets the server names.
        /// </summary>
        /// <returns></returns>
        public string[] GetServerBrokerNames()
        {
            return _serverConfigurations.Keys.ToArray();
        }

        #endregion Servers

        #region Publishers

        /// <summary>
        /// Create publisher from specified configuration key publisher
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        public IBrokerPublisher CreatePublisher(string publisherName)
        {

            if (!_brokerPublishConfigurations.TryGetValue(publisherName, out BrokerPublishParameter publisher))
                throw new Exceptions.InvalidConfigurationException($"configuration publisher {publisherName}");

            if (!_serverConfigurations.TryGetValue(publisher.ServerName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {publisher.ServerName}");

            var _broker = new RabbitBroker(server);
            var _publisher = _broker.GetPublisher(publisher);

            return _publisher;

        }

        /// <summary>
        /// Check if the configuration contains the specified publisher key
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        public Exception CheckPublisher(string publisherName)
        {

            if (!_brokerPublishConfigurations.TryGetValue(publisherName, out BrokerPublishParameter publisher))
                return new Exceptions.InvalidConfigurationException($"configuration publisher {publisherName}");

            if (!_serverConfigurations.TryGetValue(publisher.ServerName, out ServerBrokerConfiguration server))
                return new Exceptions.InvalidConfigurationException($"configuration server {publisher.ServerName}");

            return null;

        }

        /// <summary>
        /// Gets the publisher names.
        /// </summary>
        /// <returns></returns>
        public string[] GetPublisherNames()
        {
            return _brokerPublishConfigurations.Keys.ToArray();
        }

        /// <summary>
        /// Gets the publisher Configuration by the name if exists.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object GetConfigurationPublisher(string publisherName)
        {
            _brokerPublishConfigurations.TryGetValue(publisherName, out BrokerPublishParameter result);
            return result;
        }

        #endregion Publishers

        #region subscribers

        /// <summary>
        /// Create subscriber from specified configuration key subscriber
        /// </summary>
        /// <param name="subscriberName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IBrokerSubscription CreateSubscription(string subscriberName, Func<IBrokerContext, Task> callback, Func<IBrokerContext> factory = null)
        {

            if (!_brokerSubscriptionConfigurations.TryGetValue(subscriberName, out BrokerSubscriptionParameter subscriberParameter))
                throw new Exceptions.InvalidConfigurationException($"configuration subscription {subscriberName}");

            if (!_serverConfigurations.TryGetValue(subscriberParameter.ServerName, out ServerBrokerConfiguration server))
                throw new Exceptions.InvalidConfigurationException($"configuration server {subscriberParameter.ServerName}");

            IBroker _broker = new RabbitBroker(server);
            var subscriber = (RabbitBrokerSubscription)_broker.Subscribe(subscriberParameter, callback, factory);
            subscriber.Factory = this;
            return subscriber;

        }

        /// <summary>
        /// Check if the configuration contains the specified subscriber key
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        public Exception CheckSubscription(string subscriberName)
        {

            if (!_brokerSubscriptionConfigurations.TryGetValue(subscriberName, out BrokerSubscriptionParameter subscriberParameter))
                return new Exceptions.InvalidConfigurationException($"configuration subscription {subscriberName}");

            if (!_serverConfigurations.TryGetValue(subscriberParameter.ServerName, out ServerBrokerConfiguration server))
                return new Exceptions.InvalidConfigurationException($"configuration server {subscriberParameter.ServerName}");

            return null;

        }

        /// <summary>
        /// Gets the registered subscriber names.
        /// </summary>
        /// <returns></returns>
        public string[] GetSubscriberNames()
        {
            return _brokerSubscriptionConfigurations.Keys.ToArray();
        }

        /// <summary>
        /// Gets the subscriber Configuration by the name if exists.
        /// </summary>
        /// <param name="subscriberName">Name of the subscriber.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object GetConfigurationSubscriber(string subscriberName)
        {
            _brokerSubscriptionConfigurations.TryGetValue(subscriberName, out BrokerSubscriptionParameter result);
            return result;
        }

        #endregion subscribers

        private Dictionary<string, ServerBrokerConfiguration> _serverConfigurations;
        private Dictionary<string, BrokerPublishParameter> _brokerPublishConfigurations;
        private Dictionary<string, BrokerSubscriptionParameter> _brokerSubscriptionConfigurations;

    }

}
