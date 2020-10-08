using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace DigWex.Api.Model
{
  public class PinModel
  {
    [JsonRequired]
    public string pin { get; set; }
  }
}
