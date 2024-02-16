using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.FsCreator;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using nxmount.Util;

namespace nxmount.Apps.AppSource
{
    public class NSPAppSource : FileAppSource
    {
        private SharedRef<IFileSystem> Fs;

        public NSPAppSource(FileInfo file) : base(file)
        {
            Console.WriteLine($"Opening NSP {File.Name}...");
            new PartitionFileSystemCreator().Create(ref Fs, ref Storage).ThrowIfFailure();
        }
        public override IFileSystem Open()
        {
            TikUtil.TryImport(Fs.Get);
            return Fs.Get;
        }

        public new void Dispose()
        {
            base.Dispose();
            Fs.Destroy();
        }
    }
}
