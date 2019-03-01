using Bb.Brokers.Configurations;
using Bb.Configurations;
using Bb.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bb.Sdk.Brokers.Configurations
{
    public class ConvertEnvironment : JsonConverter
    {

        public ConvertEnvironment()
        {
        }

        public override bool CanConvert(Type objectType)
        {

            if (objectType == typeof(ServerBrokerConfiguration))
                return false;

            if (objectType == typeof(BrokerPublishParameter))
                return false;

            if (objectType == typeof(BrokerSubscriptionParameter))
                return false;

            return true;

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            var value = reader.Value;

            if (value == null)
                return value;

            var v = value.ToString();

            if (v.StartsWith("@"))
            {
                var key = v.Substring(1);
                value = Environment.GetEnvironmentVariable(key);
                if (value == null)
                    if (!Environment.GetEnvironmentVariables().Contains(key))
                        Trace.WriteLine($"missing environment variable {key}", TraceLevel.Error.ToString());
            }

            if (value.GetType() != objectType)
            {

                if (objectType.IsGenericType)
                {
                    Type genericTypeDefinition = objectType.GetGenericTypeDefinition();
                    Type[] genericArguments = objectType.GetGenericArguments();

                    if (genericTypeDefinition == typeof(Nullable<>))
                    {
                        value = Convert.ChangeType(value, genericArguments[0]);
                        value = Activator.CreateInstance(objectType, new object[] { value });
                    }
                    else
                        Trace.WriteLine($"Not implemented convertion of type {value.GetType().Name} in {objectType.Name}", TraceLevel.Error.ToString());
                }
                else
                    try
                    {
                        value = Convert.ChangeType(value, objectType);
                    }
                    catch (Exception)
                    {
                        var msg = $"data {value} can't be converted in {objectType}";
                        Trace.WriteLine(msg, TraceLevel.Error.ToString());
                        throw new InvalidConfigurationException(msg);
                    }
            }

            return value;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }

}
