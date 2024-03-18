using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using nxmount.Util;

namespace nxmount.Apps.Sources
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
