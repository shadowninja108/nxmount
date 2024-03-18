using nxmount.Apps;
using nxmount.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nxmount.Windows.Driver;

namespace nxmount.Windows
{
    public class MountDriver
    {
        public static IMountService Create(AppManager appManager, ApplicationLanguage desiredLanguage)
        {
            return new WinFspService(appManager, desiredLanguage);
        }
    }
}
