using System.Security.AccessControl;
using DokanNet;
using nxmount.Apps;
using nxmount.Driver;
using nxmount.Driver.Interfaces;
using nxmount.Util;
using FileAccess = DokanNet.FileAccess;

namespace nxmount.Windows.Driver
{
    public class DokanDriver : DriverBase<NtStatus>, IDokanOperations
    {

        private struct DokanFileInfo : IFileInfo
        {
            internal FileInformation Impl;

            public DokanFileInfo()
            {
                Impl = default;
                Impl.LastAccessTime = DateTime.Now;
                Impl.LastWriteTime = null;
                Impl.CreationTime = null;
            }

            public int FileNameHash { get; private set; }

            public string FileName
            {
                readonly get => Impl.FileName;
                set
                {
                    FileNameHash = value.GetHashCode();
                    Impl.FileName = value;
                }
            }

            public FileAttributes Attributes
            {
                readonly get => Impl.Attributes;
                set => Impl.Attributes = value;
            }
            public long Length
            {
                readonly get => Impl.Length;
                set => Impl.Length = value;
            }
        }
        private class DokanContext(AppManager manager, ApplicationLanguage desiredLanguage) : Context(manager, desiredLanguage)
        {
            public override IFileInfo CreateNewFileInfo(bool isDirectory = false)
            {
                IFileInfo info = new DokanFileInfo();
                FileInfoUtils.PopulateFileInfo(ref info, isDirectory);
                return info;
            }
        }

        public override NtStatus ResultSuccess => DokanResult.Success;
        public override NtStatus ResultFileNotFound => DokanResult.FileNotFound;
        public override NtStatus ResultPathNotFound => DokanResult.PathNotFound;
        public override NtStatus ResultNotImplemented => DokanResult.NotImplemented;

        public DokanDriver(AppManager manager, ApplicationLanguage desiredLanguage) : base(new DokanContext(manager, desiredLanguage))
        {
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options,
            FileAttributes attributes, IDokanFileInfo info)
        {
            if (info.IsDirectory && mode == FileMode.CreateNew)
                return DokanResult.AccessDenied;

            var result = TryGetHandler(out _, fileName, out var handler);
            if (result != NtStatus.Success)
                return result;

            info.IsDirectory = handler is IDirectoryHandler;
            info.Context = handler;

            return DokanResult.Success;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            bytesRead = 0;

            var handler = info.Context as IFsNodeHandler;
            if (handler == null)
            {
                var result = TryGetHandler(out _, fileName, out handler);
                if (result != NtStatus.Success)
                    return result;
                info.Context = handler;
            }

            if (handler is not IFileNodeHandler file)
                return DokanResult.Error;

            if (!file.Read(buffer, out bytesRead, offset))
                return DokanResult.Error;

            return DokanResult.Success;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = new List<FileInformation>();

            var handler = info.Context as IFsNodeHandler;
            var split = Span<string>.Empty;
            if (handler == null)
            {
                var result = TryGetHandler(out split, fileName, out handler);
                if (result != NtStatus.Success)
                    return result;
                info.Context = handler;
            }

            if (handler is not IDirectoryHandler dir)
                return DokanResult.NotADirectory;

            try
            {
                var query = dir.QueryChildren(Context, split);
                foreach (var fileInfo in query)
                {
                    if (fileInfo is not DokanFileInfo fi)
                        return DokanResult.Error;

                    files.Add(fi.Impl);
                }
                return DokanResult.Success;
            }
            catch (QueryFailedException)
            {
                return DokanResult.NotImplemented;
            }
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            fileInfo = default;

            var handler = info.Context as IFsNodeHandler;
            if (handler == null)
            {
                var result = TryGetHandler(out _, fileName, out handler);
                if (result != NtStatus.Success)
                    return result;
                info.Context = handler;
            }

            if (!handler!.QueryFileInfo(Context, out var fi))
                return DokanResult.Error;

            if (fi is not DokanFileInfo dfi)
                return DokanResult.Error;

            fileInfo = dfi.Impl;

            return DokanResult.Success;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName,
            out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = "nxmount";
            features = FileSystemFeatures.UnicodeOnDisk | FileSystemFeatures.ReadOnlyVolume;
            fileSystemName = "nxmount";
            maximumComponentLength = 256;
            return DokanResult.Success;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
            IDokanFileInfo info)
        {
            freeBytesAvailable = 0;
            totalNumberOfBytes = 0;
            totalNumberOfFreeBytes = 0;
            return DokanResult.Success;
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
            var context = info.Context as IFsNodeHandler;
            if (context == null)
                return;

            // context.Parent?.DisposeChild(context);
            // context.Dispose();
            info.Context = null;
        }

        #region Stubs
        public NtStatus Mounted(string mountPoint, IDokanFileInfo info) => DokanResult.Success;

        public NtStatus Unmounted(IDokanFileInfo info) => DokanResult.Success;

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = Array.Empty<FileInformation>();
            return DokanResult.NotImplemented;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = Array.Empty<FileInformation>();
            return DokanResult.NotImplemented;
        }
        #endregion

        #region Writing
        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }
        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanResult.NotImplemented;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime,
            IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return DokanResult.NotImplemented;
        }
        #endregion

        #region Unsupported
        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            security = null;
            return DokanResult.Error;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info) => DokanResult.Error;

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info) => DokanResult.Success;

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info) => DokanResult.Success;

        #endregion
    }
}
