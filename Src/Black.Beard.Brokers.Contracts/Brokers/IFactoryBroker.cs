using System;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    public interface IFactoryBroker
    {

        /// <summary>
        /// Append a new configuration server
        /// </summary>
        /// <param name="configuration"></param>
        IFactoryBroker Add(object configuration);

        /// <summary>
        /// Create broker server from specified configuration server name
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        IBroker CreateServerBroker(string serverName);

        /// <summary>
        /// Check if the configuration contains the specified borker server key
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        Exception CheckServerBroker(string serverName);

        /// <summary>
        /// Gets the publisher configuration by the name if exists.
        /// </summary>
        /// <param name="publisherName">Name of the publisher.</param>
        /// <returns></returns>
        object GetConfigurationPublisher(string publisherName);

        /// <summary>
        /// Create publisher from specified configuration key publisher
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        IBrokerPublisher CreatePublisher(string publisherName);

        /// <summary>
        /// Check if the configuration contains the specified publisher key
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        Exception CheckPublisher(string publisherName);

        /// <summary>
        /// Gets the regitered publisher names.
        /// </summary>
        /// <returns></returns>
        string[] GetPublisherNames();

        /// <summary>
        /// Gets the regitered subscriber namess.
        /// </summary>
        /// <returns></returns>
        string[] GetSubscriberNames();

        /// <summary>
        /// Gets the subscriber configuration by the name if exists.
        /// </summary>
        /// <param name="subscriberName">Name of the subscriber.</param>
        /// <returns></returns>
        object GetConfigurationSubscriber(string subscriberName);

        /// <summary>
        /// Gets the regitered server names.
        /// </summary>
        /// <returns></returns>
        string[] GetServerBrokerNames();

        /// <summary>
        /// Create subscriber from specified configuration key subscriber
        /// </summary>
        /// <param name="subscriberName">Name of the subscriber.</param>
        /// <param name="callback">The callback that contains business code.</param>
        /// <param name="factory">The factory is optional if you want override context. by default the value is () =&gt; new <see cref="!:Bb.Brokers.RabbitBrokerContext" />()</param>
        /// <returns></returns>
        IBrokerSubscription CreateSubscription(string subscriberName, Func<IBrokerContext, Task> callback, Func<IBrokerContext> factory = null);

        /// <summary>
        /// Check if the configuration contains the specified subscriber key
        /// </summary>
        /// <param name="publisherName"></param>
        /// <returns></returns>
        Exception CheckSubscription(string subscriberName);

    }

}