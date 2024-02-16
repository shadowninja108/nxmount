using LibHac.FsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Fs.Fsa;

namespace nxmount.Util
{
    public static class FileExtensions
    {
        public static DirectoryInfo GetDirectory(this DirectoryInfo dir, string sub)
        {
            return new DirectoryInfo(Path.Combine(dir.FullName, sub));
        }

        public static FileInfo GetFile(this DirectoryInfo dir, string name)
        {
            return new FileInfo(Path.Combine(dir.FullName, name));
        }

        public static IFileSystem AsLibHacFs(this DirectoryInfo directory)
        {
            return new LocalFileSystem(directory.FullName);
        }
    }
}
