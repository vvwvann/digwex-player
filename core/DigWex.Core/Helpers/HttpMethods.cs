using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace DigWex.Helpers
{
  public static class HttpMethods
  {
    public static void Upload(string actionUrl, Stream stream, MultipartFormDataContent formData, Dictionary<string, string> headers = null)
    {
      try {
        using (var client = new HttpClient()) {
          if (headers != null) {
            foreach (var item in headers)
              client.DefaultRequestHeaders.Add(item.Key, item.Value);
          }
          var response = client.PostAsync(actionUrl, formData).Result;
        }
      }
      catch (Exception) {

      }
    }
  }
}
