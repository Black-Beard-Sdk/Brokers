using Bb.Configurations;
using Bb.Exceptions;
using Bb.Sdk.Brokers.Configurations;
using Newtonsoft.Json;
using System.IO;

namespace Bb.Brokers
{

    /// <summary>
    /// helpers for to register configuration in <see cref="IFactoryBroker" />
    /// </summary>
    public static class BrokerConfigurationHelper
    {

        /// <summary>
        /// Initializes the <see cref="BrokerConfigurationHelper"/> class.
        /// </summary>
        static BrokerConfigurationHelper()
        {
            BrokerConfigurationHelper._settingsToLoad = new JsonSerializerSettings() { };
            BrokerConfigurationHelper._settingsToLoad.Converters.Add(new ConvertEnvironment());
            BrokerConfigurationHelper._settingsToSave = new JsonSerializerSettings() { };
        }

        /// <summary>
        /// Load configuration for rabbit
        /// </summary>
        /// <param name="selfPath"></param>
        /// <param name="factoryBroker"></param>
        /// <param name="serverPatternExtension"></param>
        /// <param name="publisherPatternExtension"></param>
        /// <param name="subscriberPatternExtension"></param>
        /// <returns>true if the directory exists</returns>
        public static bool LoadFolderFromJson(this string selfPath,
            IFactoryBroker factoryBroker,
            string serverPatternExtension = "*.rabbit.server",
            string publisherPatternExtension = "*.rabbit.publisher",
            string subscriberPatternExtension = "*.rabbit.subscriber")
        {

            DirectoryInfo dir = new DirectoryInfo(selfPath);
            dir.Refresh();
            if (dir.Exists)
            {
                foreach (var file in dir.GetFiles(serverPatternExtension, SearchOption.TopDirectoryOnly))
                    factoryBroker.Add(LoadFromJSon<ServerBrokerConfiguration>(File.ReadAllText(file.FullName)));

                foreach (var file in dir.GetFiles(publisherPatternExtension, SearchOption.TopDirectoryOnly))
                    factoryBroker.Add(LoadFromJSon<BrokerPublishParameter>(File.ReadAllText(file.FullName)));

                foreach (var file in dir.GetFiles(subscriberPatternExtension, SearchOption.TopDirectoryOnly))
                    factoryBroker.Add(LoadFromJSon<BrokerSubscriptionParameter>(File.ReadAllText(file.FullName)));

                return true;
            }

            return false;

        }



        /// <summary>
        /// Loads the subscriber from json.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static BrokerSubscriptionParameter LoadSubscriberFromJson(this string payload)
        {
            return LoadFromJSon<BrokerSubscriptionParameter>(payload);
        }


        /// <summary>
        /// Loads the publisher from json.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static BrokerPublishParameter LoadPublisherFromJson(this string payload)
        {
            return LoadFromJSon<BrokerPublishParameter>(payload);
        }

        /// <summary>
        /// Loads the server from json.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static ServerBrokerConfiguration LoadServerFromJson(this string payload)
        {
            return LoadFromJSon<ServerBrokerConfiguration>(payload);
        }

        /// <summary>
        /// Converts configuration to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">The self.</param>
        /// <returns></returns>
        public static string SaveToJSon<T>(this T self)
        {
            var config = Newtonsoft.Json.JsonConvert.SerializeObject(self, BrokerConfigurationHelper._settingsToSave);
            return config;
        }

        private static T LoadFromJSon<T>(string payload)
        {
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(payload, BrokerConfigurationHelper._settingsToLoad);
            return config;
        }



        /// <summary>
        /// Adds the subscriber from connection string.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static IFactoryBroker AddSubscriberFromConnectionString(this IFactoryBroker self, string payload)
        {
            self.Add(payload.LoadSubscriberFromConnectionString());
            return self;
        }

        /// <summary>
        /// Loads the subscriber from connection string.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        /// <exception cref="InvalidConfigurationException">Failed to load configuration {payload}</exception>
        public static BrokerSubscriptionParameter LoadSubscriberFromConnectionString(this string payload)
        {
            var config = new BrokerSubscriptionParameter();
            if (!LoadFromConnectionString<BrokerSubscriptionParameter>(payload, config))
                throw new InvalidConfigurationException($"Failed to load configuration {payload}");
            return config;
        }

        /// <summary>
        /// Adds the publisher from connection string.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static IFactoryBroker AddPublisherFromConnectionString(this IFactoryBroker self, string payload)
        {
            self.Add(payload.LoadPublisherFromConnectionString());
            return self;
        }

        /// <summary>
        /// Loads the publisher from connection string.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        /// <exception cref="InvalidConfigurationException">Failed to load configuration {payload}</exception>
        public static BrokerPublishParameter LoadPublisherFromConnectionString(this string payload)
        {
            var config = new BrokerPublishParameter();
            if (!LoadFromConnectionString<BrokerPublishParameter>(payload, config))
                throw new InvalidConfigurationException($"Failed to load configuration {payload}");
            return config;
        }

        /// <summary>
        /// Adds the server from connection string.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public static IFactoryBroker AddServerFromConnectionString(this IFactoryBroker self, string payload)
        {
            self.Add(payload.LoadServerFromConnectionString());
            return self;
        }

        /// <summary>
        /// Loads the server from connection string.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        /// <exception cref="InvalidConfigurationException">Failed to load configuration {payload}</exception>
        public static ServerBrokerConfiguration LoadServerFromConnectionString(this string payload)
        {
            var config = new ServerBrokerConfiguration();
            if (!LoadFromConnectionString<ServerBrokerConfiguration>(payload, config))
                throw new InvalidConfigurationException($"Failed to load configuration {payload}");
            return config;
        }

        /// <summary>
        /// Loads from connection string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload">The payload.</param>
        /// <param name="self">The self.</param>
        /// <returns></returns>
        private static bool LoadFromConnectionString<T>(string payload, T self)
        {
            return ConnectionStringHelper.Map<T>(self, payload, false);
        }

        private static readonly JsonSerializerSettings _settingsToSave;
        private static readonly JsonSerializerSettings _settingsToLoad;

    }

}
