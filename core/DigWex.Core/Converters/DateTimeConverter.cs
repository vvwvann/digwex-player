using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DigWex.Converters
{
    public class CustomDateFormat : IsoDateTimeConverter
    {
        public CustomDateFormat()
        {
            string format = "yyyy-MM-ddTHH:mm:ss";
            DateTimeFormat = format;
        }
    }

    public class IntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {

            return objectType == typeof(int[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                object result = serializer.Deserialize(reader, objectType);
                return result;
            }
            if (reader.TokenType == JsonToken.Integer)
            {
                int value = serializer.Deserialize<int>(reader);
                return new int[] { value };
            }
            return null;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            //Console.WriteLine(value.GetType());
            //var arr = value as int[];
            //writer.WriteStartObject();
            //writer.WritePropertyName("$" + name.Name);



            if (value is int[] arr)
            {
                //writer.WriteStartObject();
                serializer.Serialize(writer, arr);
                //writer.WriteEndObject();
            }
        }
    }
}
