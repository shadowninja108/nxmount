using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver
{
    public abstract class Context(AppManager manager, ApplicationLanguage desiredLanguage)
    {
        public AppManager Manager = manager;
        public ApplicationLanguage DesiredLanguage = desiredLanguage;

        public abstract IFileInfo CreateNewFileInfo(bool isDirectory = false);
    }
}
