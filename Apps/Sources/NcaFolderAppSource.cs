using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Common;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using nxmount.Util;

namespace nxmount.Apps.AppSource
{
    public class NcaFolderAppSource : IAppSource
    {
        public string Name => Info.Name;

        private DirectoryInfo Info;
        private LocalFileSystem Fs;

        public NcaFolderAppSource(DirectoryInfo info)
        {
            Info = info;
            LocalFileSystem.Create(out Fs, info.FullName).ThrowIfFailure();
        }


        public IFileSystem Open()
        {
            return new ShallowFileSystemWrapper(Fs);
        }
    }
}
