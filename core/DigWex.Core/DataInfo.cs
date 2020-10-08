using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static DigWex.Api.Model.PackageModel;
using DigWex.Managers;

namespace DigWex
{
  public class DataInfo
  {
    private readonly HashSet<int> _set;

    private JToken _data;

    public int _hash;

    public JToken Data => _data;
    private readonly DefaultPackageManager _packageManager
      = (DefaultPackageManager)PackageManager.Instance;


    private DataInfo()
    {
      _set = new HashSet<int>();
    }

    public bool Exist(int id)
    {
      return _set.Contains(id);
    }

    public static DataInfo TryCreate(DataModel data, JToken template)
    {
      DataInfo info = null;
      if (data != null) {
        info = new DataInfo {
          _data = data.Data
        };
        info._hash = data.Data.GetHashCode();
      }
      else if (template != null) {
        info = new DataInfo();
        info.Dfs(template);
        info._data = template;
        info._hash = template.GetHashCode();
      }
      return info;
    }

    public void Dfs(JToken token)
    {
      if (token == null) return;
      if (token.Type == JTokenType.String && ((string)token)[0] == '#' && ((string)token)[1] == '{') {
        string str = (string)token;
        str = str.Substring(2, str.Length - 4);
        if (int.TryParse(str, out int id)) {
          var data = _packageManager.GetValuesData(id);
          if (data != null) {
            token.Replace(data.Data);
          }
        }
      }

      JEnumerable<JToken> children = token.Children();
      foreach (var item in children) {
        Dfs(item);
      }
    }
  }
}
