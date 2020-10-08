using Newtonsoft.Json;
using System;
using DigWex.Extensions;

namespace DigWex.Converters
{
    public class UrlConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string value = serializer.Deserialize<string>(reader);
                if (value != null && value != "")
                {
                    value = value.ToValidateUrl();
                    return value;
                }
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is string url)
            {
                serializer.Serialize(writer, url);
            }
        }
    }
}
