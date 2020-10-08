using Newtonsoft.Json.Linq;

namespace DigWex.Utils
{
    public class JsonUtils
    {
        public static JToken TryParse(string str)
        {
            try
            {
                return JToken.Parse(str);
            }
            catch { }
            return null;
        }
    }
}
