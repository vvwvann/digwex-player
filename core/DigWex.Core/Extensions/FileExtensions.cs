using System.IO;
using System.Threading.Tasks;

namespace DigWex.Extensions
{
  public static class FileExtensions
  {
    public static Task DeleteAsync(this FileInfo fi)
    {
      return Task.Factory.StartNew(() => {
        fi.Delete();
      });
    }
  }
}
