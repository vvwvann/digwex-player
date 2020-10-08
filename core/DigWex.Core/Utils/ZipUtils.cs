using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigWex.Utils
{
    public static class ZipUtils
    {
        public static void Unzip(string zipPath, string extractPath)
        {
            bool ok = Directory.Exists(extractPath);

            if (ok)
                Directory.Delete(extractPath, true);
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }
    }
}
