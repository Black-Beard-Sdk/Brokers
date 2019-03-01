using Bb.Brokers.Configurations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Bb.Sdk.Brokers.Configurations
{
    public static class ConnectionStringHelper
    {

        public static string GenerateHelp(Type self)
        {

            Dictionary<string, Property> properties = self.GetPropertiesAccessor();

            StringBuilder sb = new StringBuilder(properties.Count() * 800);
            sb.AppendLine(string.Empty);

            foreach (var item in properties)
            {

                Property property = item.Value;

                if (!property.CanWrite)
                    continue;

                sb.AppendLine(property.Name);
                sb.AppendLine($"\ttype : {property.Type.Name}");

                if (property.Required)
                    sb.AppendLine("\tRequired");

                if (!string.IsNullOrEmpty(property.DisplayDescription))
                    sb.AppendLine($"\tdescription : {property.DisplayDescription}");

                //if (property.DefaultValue != null)
                //    sb.AppendLine($"\tdefault value : {property.DefaultValue}");

                if (property.Type.IsEnum)
                {
                    sb.Append("\tRestricted values (");
                    foreach (var item2 in Enum.GetNames(property.Type))
                        sb.Append($"{item2}, ");
                    sb.Remove(sb.Length - 2, 2);
                    sb.AppendLine(")");
                }

            }

            return sb.ToString();

        }

        public static Dictionary<string, string> ToDictionary(string arguments)
        {

            Dictionary<string, string> result = new Dictionary<string, string>();

            var members = arguments.Split(';');

            foreach (var item in members)
            {

                var ii = item.Split('=');

                var key = ii[0];
                var valueText = string.Join('=', ii.Skip(1));

                if (valueText.StartsWith("@"))
                {
                    var newValueText = Environment.GetEnvironmentVariable(valueText.Substring(1));
                    if (!string.IsNullOrEmpty(newValueText))
                        valueText = newValueText;
                }
                else
                    Trace.WriteLine($"environment variable {valueText} not found", TraceLevel.Warning.ToString());

                result.Add(key, valueText);

            }

            return result;

        }

        public static bool Map<T>(T self, string arguments, bool respectCase = false)
        {

            Dictionary<string, Property> properties = self.GetType().GetPropertiesAccessor();

            var members = arguments.Split(';');

            foreach (var item in members)
            {

                if (string.IsNullOrEmpty(item))
                    continue;

                var ii = item.Split('=');
                var key = ii[0].Trim();
                var valueText = string.Join('=', ii.Skip(1));
                if (string.IsNullOrEmpty(valueText))
                    continue;

                valueText = valueText.Trim();

                Property property;
                if (respectCase)
                    property = properties[key];
                else
                    property = properties.FirstOrDefault(c => c.Key.ToLower() == key.ToLower()).Value;

                if (property != null)
                {

                    if ((typeof(string) == property.Type || property.Type.IsValueType) && property.CanWrite)
                    {

                        if (valueText.StartsWith("@"))
                        {
                            var newValueText = Environment.GetEnvironmentVariable(valueText.Substring(1));
                            if (!string.IsNullOrEmpty(newValueText))
                                valueText = newValueText;
                            else
                                Trace.WriteLine($"environment variable {valueText} not found", TraceLevel.Warning.ToString());
                        }

                        object value = MyConverter.Unserialize(valueText, property.Type);
                        property.Set(self, value);

                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.Type) && property.Type.IsGenericType)
                    {

                        var p = property.Type.GetGenericArguments();
                        if (p.Length > 1)
                        {

                        }

                        var type = p[0];

                        dynamic value = property.Get(self);

                        if (value == null)
                        {
                            if (property.CanWrite)
                            {
                                value = Activator.CreateInstance(property.Type);
                                property.Set(self, value);
                            }
                            else
                                continue;
                        }

                        var jj = valueText.Split(',');
                        foreach (var item3 in jj)
                        {

                            string subValueText = item3.Trim();

                            if (item3.StartsWith("@"))
                            {
                                var newValueText = Environment.GetEnvironmentVariable(item3.Substring(1));
                                if (!string.IsNullOrEmpty(newValueText))
                                    subValueText = newValueText;
                                else
                                    Trace.WriteLine($"environment variable {item3} not found", TraceLevel.Warning.ToString());
                            }

                            dynamic value2 = MyConverter.Unserialize(subValueText, type).Trim();
                            value.Add(value2);
                        }

                    }

                }

            }

            bool result = true;

            foreach (var item in properties)
            {
                if (item.Value.Required && item.Value.Get(self) == null)
                {
                    Trace.WriteLine($"{item.Value.Name} is required");
                    result = false;
                }
            }

            return result;

        }

    }


}
