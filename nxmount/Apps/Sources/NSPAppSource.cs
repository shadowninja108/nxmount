using LibHac.Common;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.FsCreator;

namespace nxmount.Apps.Sources
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
            return Fs.Get;
        }

        public new void Dispose()
        {
            base.Dispose();
            Fs.Destroy();
        }
    }
}
