using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DigWex.Api.Model.PackageModel;

namespace DigWex.Model
{
  public class ItemModel
  {
    public ContentModel Content { get; set; }

    public int[] Position { get; set; }

    public DataModel Data { get; set; }

    public JToken Template { get; set; }
  }
}
