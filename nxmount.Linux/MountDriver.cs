using nxmount.Apps;
using nxmount.Util;
using nxmount.Linux.Driver;

namespace nxmount.Linux
{
    public class MountDriver
    {
        public static IMountService Create(AppManager appManager, ApplicationLanguage desiredLanguage)
        {
            return new NxFuseService(appManager, desiredLanguage, "/tmp/mount/");
        }
    }
}
