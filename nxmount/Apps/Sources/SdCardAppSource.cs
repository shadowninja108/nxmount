using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using nxmount.Util;

namespace nxmount.Apps.Sources
{
    public class SdCardAppSource : IAppSource
    {
        public string Name => "SD Card";

        private readonly DirectoryInfo Info;
        private readonly UniqueRef<IAttributeFileSystem> LocalFs;
        private readonly AesXtsFileSystem EncFs;

        public SdCardAppSource(DirectoryInfo info)
        {
            Info = info;

            LocalFileSystem.Create(out var localFs, info.FullName).ThrowIfFailure();
            LocalFs = new UniqueRef<IAttributeFileSystem>(localFs);

            var concatFs = new ConcatenationFileSystem(ref LocalFs);

            using var contentDirPath = new LibHac.Fs.Path();
            PathFunctions.SetUpFixedPath(ref contentDirPath.Ref(), "/Nintendo/Contents"u8).ThrowIfFailure();

            var contentDirFs = new SubdirectoryFileSystem(concatFs);
            contentDirFs.Initialize(in contentDirPath).ThrowIfFailure();

            EncFs = new AesXtsFileSystem(contentDirFs, Keyset.Instance.SdCardEncryptionKeys[1].DataRo.ToArray(), 0x4000);
        }

        public IFileSystem Open()
        {
            return EncFs;
        }
    }
}
