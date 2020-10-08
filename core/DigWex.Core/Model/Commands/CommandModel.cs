using Newtonsoft.Json;
using DigWex.Model.Commands;

namespace DigWex.Model
{
    public class CommandModel
    {
        [JsonRequired]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonRequired]
        [JsonProperty("command")]
        public string Command { get; set; }
    }

    public abstract class CommandModel<T> : CommandModel where T : IKWargs
    {
        [JsonProperty("kwargs")]
        public T Kwargs { get; set; }
    }
}
