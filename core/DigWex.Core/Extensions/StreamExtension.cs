using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DigWex.Extensions
{
  public static class StreamExtension
  {
    public static async Task<int> ReadTimeoutAsync(this Stream stream, byte[] buffer, int offset,
        int count, int timeout)
    {
      using (var cancellationTokenSource = new CancellationTokenSource(timeout)) {
        using (cancellationTokenSource.Token.Register(() => stream.Close())) {
          return await stream.ReadAsync(buffer, offset, count);
        }
      }
    }

    public static async Task<HttpWebResponse> GetResponseTimeoutAsync(this HttpWebRequest request)
    {
      using (var cancellationTokenSource = new CancellationTokenSource(request.Timeout)) {
        using (cancellationTokenSource.Token.Register(() => request.Abort())) {
          return (HttpWebResponse)await request.GetResponseAsync();
        }
      }
    }
  }
}