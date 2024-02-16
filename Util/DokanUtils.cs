using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Util
{
    public static class DokanUtils
    {
        public static FileInformation CreateFileInfo(bool isDirectory)
        {
            var info = new FileInformation();
            info.PopulateFileInfo(isDirectory);
            return info;
        }

        public static void PopulateFileInfo(this ref FileInformation info, bool isDirectory)
        {
            info.Attributes = (isDirectory ? FileAttributes.Directory : FileAttributes.Normal) | FileAttributes.ReadOnly;
            info.LastAccessTime = DateTime.Now;
            info.LastWriteTime = null;
            info.CreationTime = null;
        }
    }
}
