using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DokanNet;
using LibHac.Bcat;
using LibHac.Fs;
using LibHac.Tools.Fs;
using LibHac.Util;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;
using FileAccess = DokanNet.FileAccess;
using Path = LibHac.Fs.Path;

namespace nxmount.Driver
{
    public class DokanDriver : IDokanOperations
    {
        private Context Context;
        private NodeListHandler Root;

        public DokanDriver(AppManager manager, ApplicationLanguage desiredLanguage)
        {
            Context = new Context(manager, desiredLanguage);
            Root = new NodeListHandler(null);
            Root.Initialize([
                new GamesHandler(Root, Context)
            ]);
        }

        private string? TryNormalizePath(string path)
        {
            path = path.Replace("\\", "/");
            var libhacPath = new Path();
            var normalizationResult = libhacPath.InitializeWithNormalization(StringUtils.StringToUtf8(path));
            if (normalizationResult.IsFailure())
            {
                Console.WriteLine($"Error: {normalizationResult}");
                return null;
            }

            var str = libhacPath.ToString();
            var regex = new Regex($"[{Regex.Escape(new string(System.IO.Path.GetInvalidPathChars()))}]");
            str = regex.Replace(str, "");
            return str;
        }

        private NtStatus TryGetHandler(out Span<string> split, string fileName, out IFsNodeHandler? handler)
        {
            split = Span<string>.Empty;
            handler = null;
            if (fileName.EndsWith("desktop.ini"))
                return DokanResult.FileNotFound;

            var path = TryNormalizePath(fileName);
            if (path == null)
                return DokanResult.PathNotFound;

            PathUtils.Split(out split, path);
            if (!Root.QueryNode(Context, split, out handler))
                return DokanResult.NotImplemented;

            if (handler == null)
                return DokanResult.PathNotFound;

            var depth = 0;
            var search = handler;
            while (search != null)
            {
                depth++;
                search = search.Parent;
            }

            split = split[(depth-1)..];

            return DokanResult.Success;
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

            var handler = (IFsNodeHandler?)info.Context;
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

            var handler = (IFsNodeHandler?)info.Context;
            var split = Span<string>.Empty;
            if (handler == null)
            {
                var result = TryGetHandler(out split, fileName, out handler);
                if (result != NtStatus.Success)
                    return result;
                info.Context = handler;
            }

            if(handler is not IDirectoryHandler dir)
                return DokanResult.NotADirectory;

            if (!dir.QueryChildren(Context, split, ref files))
                return DokanResult.NotImplemented;

            return DokanResult.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            fileInfo = default;

            var handler = (IFsNodeHandler?)info.Context;
            if (handler == null)
            {
                var result = TryGetHandler(out _, fileName, out handler);
                if (result != NtStatus.Success)
                    return result;
                info.Context = handler;
            }

            if (!handler!.QueryFileInfo(Context, ref fileInfo))
                return DokanResult.Error;

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
