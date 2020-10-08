using System;
using System.Net;
using DigWex.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using DigWex.Api.Model;
using VvHelpers.Extensions;

namespace DigWex.Api
{
  public class DeviceApi
  {

#if ADVELIT
    private const string ROUTE_DEVICE = "/external/api/v1/device";
    private const string ENDPOINT = "/external/api/v1";
#else
    private const string ENDPOINT = "/api/v1";
    private const string ROUTE_DEVICE = "/api/v1/device";
#endif

    private readonly ActivateModel.Response _activateData;
    private static readonly Lazy<DeviceApi> _instance =
        new Lazy<DeviceApi>(() => new DeviceApi());
    private readonly string _serverUrl;

    public static DeviceApi Instance => _instance.Value;

    // 0 - activate
    private readonly string[] _actionUrls = new string[2];
    private readonly Dictionary<string, string> _headers;

    private DeviceApi()
    {
      _serverUrl = Config.ServerUrl.Trim('/');
      string prefix = $"{_serverUrl}{ROUTE_DEVICE}";
      _activateData = Config.ActivateData;
      if (_activateData != null) {
        _headers = new Dictionary<string, string> {
          ["Authorization"] = "Bearer " + _activateData.Token
        };
        _actionUrls[0] = $"{prefix}/{_activateData.Id}/package";
      }
    }

    public async Task<ResultPack> DataAsync()
    {
      var msg = await VvHelpers.HttpMethods.GetAsync(_actionUrls[0], _headers);

      if (msg == null)
        return new ResultPack(HttpStatusCode.BadGateway, null);

      if (msg.StatusCode == HttpStatusCode.OK) {
        try {
          var model = JsonConvert.DeserializeObject<PackageModel>(msg.Body);
          return new ResultPack(msg.StatusCode, model);
        }
        catch {
          Log.Warn("The package data does not match the JSON format");
        }
      }
      return new ResultPack(msg.StatusCode, null);
    }

    public async Task<bool> TokenAsync()
    {
      VvHelpers.ResponseHttp response = await VvHelpers.HttpMethods
        .GetAsync($"{_serverUrl}{ENDPOINT}/tokens", _headers);
      return response == null ? true : response.StatusCode != HttpStatusCode.Unauthorized;
    }

    public void Upload(string url, Stream stream, MultipartFormDataContent formData)
    {
      HttpMethods.Upload(url, stream, formData, _headers);
    }

    public void UploadScreenshot(Stream stream)
    {
      string url = $"{_serverUrl}{ROUTE_DEVICE}/{_activateData.Id}/screen";
      HttpContent streamContent = new StreamContent(stream);
      streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

      using (var formData = new MultipartFormDataContent()) {
        formData.Add(streamContent, "file", "file.jpg");
        Upload(url, stream, formData);
      }
    }

    public async Task<TimeModel.Response> GetServerTime()
    {
      string url = $"{_serverUrl}{ROUTE_DEVICE}/{_activateData.Id}/time";

      try {
        var uri = new UriBuilder(url) {
          Scheme = Uri.UriSchemeHttp,
          Port = -1
        };
        url = uri.ToString();
      }
      catch (Exception ex) {
        Log.Warn("Url server time parse exception: " + ex.Message);
        return null;
      }

      var msg = await VvHelpers.HttpMethods.GetAsync(url,
        new Dictionary<string, string> { ["Authorization"] = $"Bearer {Config.ActivateData.Token}" });
      return msg?.StatusCode == HttpStatusCode.OK ? msg.Body.ToJsonObject<TimeModel.Response>() : null;
    }

    public async Task<SplashModel> GetSplashAsync()
    {
      string url = $"{_serverUrl}{ROUTE_DEVICE}/{_activateData.Id}/splash";

      try {
        var msg = await VvHelpers.HttpMethods.GetAsync(url);
        if (msg.StatusCode == HttpStatusCode.OK) {
          return JsonConvert.DeserializeObject<SplashModel>(msg.Body);
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
      }
      return null;
    }

    public void UploadLog(Stream stream)
    {
      string url = $"{_serverUrl}{ROUTE_DEVICE}/{_activateData.Id}/log";
      HttpContent streamContent = new StreamContent(stream);

      try {
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

        using (var formData = new MultipartFormDataContent()) {
          formData.Add(streamContent, "file", "log.zip");
          Upload(url, stream, formData);
        }
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
      }
    }

    private static void SetHeaders(Dictionary<string, string> headers, HttpWebRequest request)
    {

      var tmp = new Dictionary<string, string>(headers);
      string[] keys = new string[] { "Content-Type", "User-Agent", "Accept" };

      int index = 0;
      foreach (string key in keys) {
        if (tmp.ContainsKey(key)) {
          //Console.WriteLine(key);
          switch (index) {
            case 0: {
                request.ContentType = headers[key];
                break;
              }
            case 1: {
                request.UserAgent = headers[key];
                break;
              }
            case 2: {
                request.Accept = headers[key];
                break;
              }
          }
          tmp.Remove(key);
        }
        index++;
      }

      foreach (var item in tmp) {
        request.Headers[item.Key] = item.Value;
      }
    }

    public async Task<bool> StatsAsync(object data)
    {
      VvHelpers.ResponseHttp response = await VvHelpers.HttpMethods.PostAsync(
        $"{_serverUrl}{ROUTE_DEVICE}/{_activateData.Id}/stats", data, _headers);

      return response?.StatusCode == HttpStatusCode.OK;
    }
  }

  public struct ResultPack
  {
    public HttpStatusCode Code;
    public PackageModel Model;

    public ResultPack(HttpStatusCode code, PackageModel model)
    {
      Code = code;
      Model = model;
    }
  }
}