using System.IO;

namespace DigWex.Utils
{
    public class DriveUtils
    {
        public static DriveInfo GetInfoByDisk(string driveName)
        {
            try
            {
                return new DriveInfo(driveName);
            }
            catch { };
            return null;
        }

    }
}
