using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;

namespace nxmount.Apps.AppSource
{
    public abstract class FileAppSource : IAppSource, IDisposable
    {
        public string Name => File.Name;

        protected FileInfo File;
        protected SharedRef<IFile> LibHacFile;
        protected SharedRef<IStorage> Storage;

        protected FileAppSource(FileInfo file)
        {
            File = file;
            LibHacFile = new SharedRef<IFile>(new LocalFile(file.FullName, OpenMode.Read));
            Storage = new SharedRef<IStorage>(new FileStorage(ref LibHacFile));
        }


        public abstract IFileSystem Open();

        public void Dispose()
        {
            LibHacFile.Destroy();
            Storage.Destroy();
        }
    }
}
