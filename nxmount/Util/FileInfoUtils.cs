using nxmount.Driver.Interfaces;

namespace nxmount.Util
{
    public static class FileInfoUtils
    {
        public static void PopulateFileInfo(ref IFileInfo info, bool isDirectory)
        {
            info.Attributes = (isDirectory ? FileAttributes.Directory : 0) | FileAttributes.ReadOnly;
        }
    }
}
