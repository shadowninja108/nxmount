using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nxmount.Apps;
using nxmount.Util;

namespace nxmount.Driver
{
    public class Context(AppManager manager, ApplicationLanguage desiredLanguage)
    {
        public AppManager Manager = manager;
        public ApplicationLanguage DesiredLanguage = desiredLanguage;
    }
}
