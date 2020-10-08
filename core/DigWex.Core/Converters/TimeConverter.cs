using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using DigWex.Model;

namespace DigWex.Converters
{
    public class CustomTimeFormat : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string time = serializer.Deserialize<string>(reader);
                return ParseTime(time);
            }
            return null;
        }

        private DateTime ParseTime(string time)
        {
            return DateTime.ParseExact(time, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime time)
            {
                serializer.Serialize(writer, time.ToString("HH:mm"));
            }
        }
    }
}
