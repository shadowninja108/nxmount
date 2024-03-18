using System.Collections.Concurrent;
using System.Collections.Immutable;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;
using Path = LibHac.Fs.Path;

namespace nxmount.Driver.Handlers
{
    public class SectionFilesystemHandler : IDirectoryHandler
    {
        public IDirectoryHandler? Parent { get; }
        public string FileName => SectionType.ToString();

        private TitleInfo Info;
        private TitleInfo.ContentKey ContentKey;
        private NcaSectionType SectionType;

        private IFileSystem FileSystem;

        private HashSet<IFsNodeHandler> OpenHandlers = new();
        private ConcurrentDictionary<string, FileContext> OpenFiles = new();
        private ConcurrentDictionary<string, DirectoryContext> Directories = new();

        private class FileContext : IFileNodeHandler
        {
            public IDirectoryHandler? Parent { get; internal set; }
            public long Size { get; internal set; }
            public required string FileName { get; internal init; }

            public UniqueRef<IFile> File;

            public bool QueryFileInfo(Context context, out IFileInfo info)
            {
                info = context.CreateNewFileInfo(false);
                info.FileName = FileName;
                info.Length = Size;
                return true;
            }

            public bool Read(Span<byte> buffer, out int bytesRead, long offset)
            {
                bytesRead = 0;

                var result = File.Get.Read(out var read, offset, buffer).Log();
                if (result.IsFailure())
                    return false;

                bytesRead = (int)read;
                return true;
            }

            public void Dispose()
            {
                File.Destroy();
            }
        }

        private class DirectoryContext : IDirectoryHandler
        {
            public string FileName => PathComponents.Last();

            public IDirectoryHandler? Parent { get; internal init; }
            public required SectionFilesystemHandler Handler { get; init; }
            public required string[] PathComponents { get; init; }

            public bool QueryFileInfo(Context context, out IFileInfo info)
            {
                info = context.CreateNewFileInfo(true);
                info.FileName = PathUtils.CleanupFilename(FileName);
                return true;
            }

            private string[] GetFullPathComponents(ReadOnlySpan<string> pathComponents)
            {
                return [.. PathComponents, .. pathComponents.ToImmutableArray()];
            }

            public IEnumerable<IFileInfo> QueryChildren(Context context, ReadOnlySpan<string> pathComponents)
            {
                var paths = GetFullPathComponents(pathComponents);
                return Handler.QueryChildren(context, paths);
            }

            public bool QueryNode(Context context, ReadOnlySpan<string> pathComponents, out IFsNodeHandler? info)
            {
                var paths = GetFullPathComponents(pathComponents);
                return Handler.QueryNode(context, paths, out info);
            }

            public void DisposeChild(IFsNodeHandler handlerToDispose) { }
            public void Dispose() { }
        }

        public SectionFilesystemHandler(IDirectoryHandler parent, TitleInfo version, TitleInfo.ContentKey contentKey, NcaSectionType sectionType)
        {
            Parent = parent;
            Info = version;
            ContentKey = contentKey;
            SectionType = sectionType;

            FileSystem = version.OpenFilesystem(contentKey, sectionType)!;
        }

        private static Path ConvertPath(ReadOnlySpan<string> pathComponents)
        {
            var path = new Path();
            path.InitializeAsEmpty();
            path.AppendChild("/"u8);
            foreach (var component in pathComponents)
            {
                path.AppendChild(component.ToU8Span());
            }
            path.Normalize(new PathFlags(PathFlags.PathFormatFlags.AllowWindowsPath)).ThrowIfFailure();

            return path;
        }

        private FileContext? OpenFile(in Path path)
        {
            var str = path.ToString();
            if (OpenFiles.TryGetValue(str, out var context))
                return context;

            context = new FileContext()
            {
                FileName = str[(str.LastIndexOf('/') + 1)..]
            };
            var openResult = FileSystem.OpenFile(ref context.File.Ref, in path, OpenMode.Read).Log();
            if (openResult.IsFailure())
                return null;

            var sizeResult = context.File.Get.GetSize(out var size).Log();
            if (sizeResult.IsFailure())
                return null;

            var parent = new Path();
            parent.Initialize(in path).ThrowIfFailure();
            parent.RemoveChild().ThrowIfFailure();
            context.Parent = OpenDirectory(ref parent);
            context.Size = size;
            OpenFiles[str] = context;
            OpenHandlers.Add(context);
            return context;
        }

        private IDirectoryHandler? OpenDirectory(ref Path path)
        {
            var pathBuf = path.GetString();
            if (!path.IsEmpty() && pathBuf[0] == '/' && pathBuf[1] == '\0')
                return this;

            var str = path.ToString();
            if (Directories.TryGetValue(str, out var context))
                return context;

            var entryTypeResult = FileSystem.GetEntryType(out var entryType, in path).Log();
            if (entryTypeResult.IsFailure())
                return null;

            if (entryType != DirectoryEntryType.Directory)
                return null;

            path.RemoveChild().ThrowIfFailure();
            context = new DirectoryContext()
            {
                Parent = OpenDirectory(ref path),
                PathComponents = str[1..].Split('/'),
                Handler = this
            };

            Directories[str] = context;
            OpenHandlers.Add(context);
            return context;
        }

        public bool QueryFileInfo(Context context, out IFileInfo info)
        {
            info = context.CreateNewFileInfo(true);
            info.FileName = SectionType.ToString();
            return true;
        }

        public IEnumerable<IFileInfo> QueryChildren(Context context, ReadOnlySpan<string> pathComponents)
        {
            using var path = ConvertPath(pathComponents);
            using var dir = new UniqueRef<IDirectory>();
            return FileSystem.EnumerateEntries(path.ToString(), "*", SearchOptions.Default).Select(entry =>
            {
                var info = context.CreateNewFileInfo(entry.Type == DirectoryEntryType.Directory);
                info.FileName = entry.Name;
                if (entry.Type == DirectoryEntryType.File)
                {
                    info.Length = entry.Size;
                }

                return info;
            });
        }

        public bool QueryNode(Context context, ReadOnlySpan<string> pathComponents, out IFsNodeHandler? info)
        {
            info = null;
            if (pathComponents.IsEmpty)
            {
                info = this;
                return true;
            }

            using var path = ConvertPath(pathComponents);

            /* Try opening as a file. */
            var file = OpenFile(in path);
            if (file != null)
            {
                /* File was opened so this is a file. */
                info = file;
                return true;
            }

            /* Try opening as a directory. */
            var workingPath = new Path();
            workingPath.Initialize(in path);
            var directory = OpenDirectory(ref workingPath);
            if (directory != null)
            {
                info = directory;
                return true;
            }


            return false;
        }

        public void DisposeChild(IFsNodeHandler handlerToDispose)
        {
        }

        public void Dispose()
        {
            foreach (var (_, file) in OpenFiles)
            {
                file.Dispose();
            }
            foreach (var (_, directory) in Directories)
            {
                directory.Dispose();
            }

            OpenFiles.Clear();
            Directories.Clear();
            OpenHandlers.Clear();
        }
    }
}
