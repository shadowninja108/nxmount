using System.Collections.Immutable;
using DokanNet;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Util;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;
using System.Linq;
using ContentType = LibHac.Ncm.ContentType;
using Path = LibHac.Fs.Path;

namespace nxmount.Driver
{
    public class SectionContentHandler : IDirectoryHandler
    {
        public IFsNodeHandler? Parent { get; }
        public string FileName => SectionType.ToString();

        private TitleInfo Info;
        private ContentType ContentType;
        private NcaSectionType SectionType;

        private IFileSystem FileSystem;

        private Dictionary<string, FileContext> OpenFiles = new();
        private Dictionary<string, DirectoryContext> Directories = new();

        class FileContext : IFileNodeHandler
        {
            public IFsNodeHandler? Parent { get; internal init; }
            public long Size { get; internal set; }
            public string FileName { get; }

            public UniqueRef<IFile> File;

            public DirectoryEntryType? EntryType;

            public bool QueryFileInfo(Context context, ref FileInformation info)
            {
                info.PopulateFileInfo(false);
                info.FileName = FileName;
                info.Length = Size;
                return true;
            }

            public bool Read(byte[] buffer, out int bytesRead, long offset)
            {
                bytesRead = 0;

                var result = File.Get.Read(out var read, offset, buffer).Log();
                if (result.IsFailure())
                    return false;

                bytesRead = (int)read;
                return true;
            }
        }

        class DirectoryContext : IDirectoryHandler
        {
            public IFsNodeHandler? Parent { get; internal init; }

            public string FileName => PathComponents.Last();
            public SectionContentHandler Handler { get; internal init; }

            public string[] PathComponents { get; internal set; }

            public bool QueryFileInfo(Context context, ref FileInformation info)
            {
                info.PopulateFileInfo(true);
                info.FileName = PathUtils.CleanupFilename(FileName);
                return true;
            }

            public bool QueryChildren(Context context, ReadOnlySpan<string> pathComponents, ref IList<FileInformation> files)
            {
                var paths = PathComponents.Concat(pathComponents.ToImmutableArray()).ToArray();
                return Handler.QueryChildren(context, paths, ref files);
            }

            public bool QueryNode(Context context, ReadOnlySpan<string> pathComponents, out IFsNodeHandler? info)
            {
                var paths = PathComponents.Concat(pathComponents.ToImmutableArray()).ToArray();
                return Handler.QueryNode(context, paths, out info);
            }
        }

        public SectionContentHandler(IFsNodeHandler parent, TitleInfo version, ContentType contentType, NcaSectionType sectionType)
        {
            Parent = parent;
            Info = version;
            ContentType = contentType;
            SectionType = sectionType;

            FileSystem = version.OpenFilesystem(contentType, sectionType)!;
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

            return path;
        }

        private FileContext? OpenFile(in Path path)
        {
            var str = path.ToString();
            if (OpenFiles.TryGetValue(str, out var context))
                return context;

            context = new FileContext();
            var openResult = FileSystem.OpenFile(ref context.File.Ref, in path, OpenMode.Read).Log();
            if (openResult.IsFailure())
                return null;

            var sizeResult = context.File.Get.GetSize(out var size).Log();
            if(sizeResult.IsFailure())
                return null;

            context.Size = size;
            OpenFiles[str] = context;
            return context;
        }

        private IDirectoryHandler? OpenDirectory(in Path path)
        {
            if (path.IsEmpty())
                return this;

            var str = path.ToString();
            if (Directories.TryGetValue(str, out var context))
                return context;

            var entryTypeResult = FileSystem.GetEntryType(out var entryType, in path).Log();
            if (entryTypeResult.IsFailure())
                return null;

            if(entryType != DirectoryEntryType.Directory)
                return null;

            using var parentPath = new Path();
            parentPath.Initialize(in path);
            parentPath.RemoveChild();
            context = new DirectoryContext()
            {
                Parent = OpenDirectory(parentPath),
                PathComponents = str.Split('/'),
                Handler = this
            };

            Directories[str] = context;
            return context;
        }

        public bool QueryFileInfo(Context context, ref FileInformation info)
        {
            info.PopulateFileInfo(true);
            info.FileName = SectionType.ToString();
            return true;
        }

        public bool QueryChildren(Context context, ReadOnlySpan<string> pathComponents, ref IList<FileInformation> files)
        {
            using var path = ConvertPath(pathComponents);
            using var dir = new UniqueRef<IDirectory>();
            var openResult = FileSystem.OpenDirectory(ref dir.Ref, in path, OpenDirectoryMode.All).Log();
            if (openResult.IsFailure())
                return false;

            var countResult = dir.Get.GetEntryCount(out var count).Log();
            if (countResult.IsFailure())
                return false;

            var buffer = new DirectoryEntry[count];
            var getResult = dir.Get.Read(out var _, buffer).Log();
            if(getResult.IsFailure())
                return false;

            foreach (var entry in buffer)
            {
                var info = DokanUtils.CreateFileInfo(entry.Type == DirectoryEntryType.Directory);
                info.FileName = StringUtils.Utf8ZToString(entry.Name);
                if (entry.Type == DirectoryEntryType.File)
                {
                    info.Length = entry.Size;
                }
                files.Add(info);
            }
            return true;
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
            var directory = OpenDirectory(in path);
            if (directory != null)
            {
                info = directory;
                return true;
            }


            return false;
        }
    }
}
