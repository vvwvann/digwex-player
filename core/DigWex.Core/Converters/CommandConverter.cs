using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using DigWex.Model;
using DigWex.Model.Commands;

namespace DigWex.Converters
{
    public class CommandConverter : JsonConverter
    {
        private static Dictionary<string, Type> SUPPORT_COMMANDS = new Dictionary<string, Type>() {
            { PowerCommandModel.Type, typeof(PowerCommandModel)},
            { SynchronizeCommandModel.Type, typeof(SynchronizeCommandModel)},
            { TakeScreenshotCommandModel.Type, typeof(TakeScreenshotCommandModel)},
            { UpdataDataCommandModel.Type, typeof(UpdataDataCommandModel)},
            { UploadLogsCommandModel.Type, typeof(UploadLogsCommandModel)},
            { DistUrlCommandModel.Type, typeof(DistUrlCommandModel)},
            { BackendUrlCommandModel.Type, typeof(BackendUrlCommandModel)},
            { RebootCommandModel.Type, typeof(RebootCommandModel)}
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<CommandModel>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var array = JArray.Load(reader);
                var list = new List<CommandModel>();
                foreach (var item in array)
                {
                    string command = (string)item["command"];
                    SUPPORT_COMMANDS.TryGetValue(command, out Type type);
                    type = type ?? typeof(CommandModel);

                    var obj = (CommandModel)item.ToObject(type);
                    if (obj != null)
                        list.Add(obj);
                }
                return list;
            }
            catch { }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }
    }
}
