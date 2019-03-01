using Bb.Brokers;
using Bb.Configurations;
using Bb.Exceptions;
using Newtonsoft.Json;
using System.IO;

namespace Bb.Sdk.Brokers.Configurations
{

    public static class BrokerConfigurationHelper
    {

        static BrokerConfigurationHelper()
        {

            BrokerConfigurationHelper._settingsToLoad = new JsonSerializerSettings()
            {

            };

            BrokerConfigurationHelper._settingsToLoad.Converters.Add(new ConvertEnvironment());

            BrokerConfigurationHelper._settingsToSave = new JsonSerializerSettings()
            {

            };


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


        public static BrokerSubscriptionParameter LoadSubscriberFromJson(this string payload)
        {
            return LoadFromJSon<BrokerSubscriptionParameter>(payload);
        }

        public static BrokerPublishParameter LoadPublisherFromJson(this string payload)
        {
            return LoadFromJSon<BrokerPublishParameter>(payload);
        }

        public static ServerBrokerConfiguration LoadServerFromJson(this string payload)
        {
            return LoadFromJSon<ServerBrokerConfiguration>(payload);
        }

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



        public static BrokerSubscriptionParameter LoadSubscriberFromConnectionString(this string payload)
        {
            var config = new BrokerSubscriptionParameter();
            if (LoadFromConnectionString<BrokerSubscriptionParameter>(payload, config))
                throw new InvalidConfigurationException($"Failed to load configuration {payload}");
            return config;
        }

        public static BrokerPublishParameter LoadPublisherFromConnectionString(this string payload)
        {
            var config = new BrokerPublishParameter();
            if (!LoadFromConnectionString<BrokerPublishParameter>(payload, config))
                throw new InvalidConfigurationException($"Failed to load configuration {payload}");
            return config;
        }

        public static ServerBrokerConfiguration LoadServerLoadFromConnectionString(this string payload)
        {
            var config = new ServerBrokerConfiguration();
            if (!LoadFromConnectionString<ServerBrokerConfiguration>(payload, config))
                throw new InvalidConfigurationException($"Failed to load configuration {payload}");
            return config;
        }

        private static bool LoadFromConnectionString<T>(string payload, T self)
        {
            return ConnectionStringHelper.Map<T>(self, payload, false);
        }

        private static readonly JsonSerializerSettings _settingsToSave;
        private static readonly JsonSerializerSettings _settingsToLoad;

    }

}
