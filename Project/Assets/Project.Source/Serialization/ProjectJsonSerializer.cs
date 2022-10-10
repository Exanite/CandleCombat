#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Project.Source.Serialization
{
    public class ProjectJsonSerializer : JsonSerializer
    {
        public T? Deserialize<T>(JToken token)
        {
            using (var reader = token.CreateReader())
            {
                return Deserialize<T>(reader);
            }
        }

        public T? PopulateWithConverter<T>(JToken token, T? value)
        {
            using (var reader = token.CreateReader())
            {
                return (T?)PopulateWithConverter(reader, value, typeof(T));
            }
        }

        public T? PopulateWithConverter<T>(JsonReader reader, T? value)
        {
            return (T?)PopulateWithConverter(reader, value, typeof(T));
        }

        public object? PopulateWithConverter(JsonReader reader, object? value, Type objectType)
        {
            JsonConverter? matchedConverter = null;

            foreach (var converter in Converters)
            {
                if (converter.CanConvert(objectType))
                {
                    matchedConverter = converter;

                    break;
                }
            }

            if (matchedConverter == null)
            {
                if (value == null)
                {
                    throw new ArgumentException("Provided value was null and no matching converter was found");
                }

                Populate(reader, value);

                return value;
            }

            return matchedConverter.ReadJson(reader, objectType, value, this);
        }
    }
}