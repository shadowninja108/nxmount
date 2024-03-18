using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Util;
using Path = LibHac.Fs.Path;

namespace nxmount.Util
{
    public class ShallowFileSystemWrapper(IFileSystem impl) : IFileSystem
    {
        private static bool IsPathNotShallow(Path path)
        {
            var s = path.ToString();
            for (var i = 1; i < s.Length; i++)
            {
                if (s[i] == '/')
                    return true;
            }
            return false;
        }

        protected override Result DoCreateFile(ref readonly Path path, long size, CreateFileOptions option) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoDeleteFile(ref readonly Path path) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoCreateDirectory(ref readonly Path path) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoDeleteDirectory(ref readonly Path path) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoDeleteDirectoryRecursively(ref readonly Path path) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoCleanDirectoryRecursively(ref readonly Path path) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoRenameFile(ref readonly Path currentPath, ref readonly Path newPath) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoRenameDirectory(ref readonly Path currentPath, ref readonly Path newPath) => ResultFs.WriteUnpermitted.Value;

        protected override Result DoGetEntryType(out DirectoryEntryType entryType, ref readonly Path path)
        {
            entryType = default;

            if (IsPathNotShallow(path)) { return ResultFs.PathNotFound.Value; }

            var result = impl.GetEntryType(out entryType, in path);
            //if (entryType == DirectoryEntryType.Directory) { return ResultFs.PathNotFound.Value; }

            return result;
        }

        protected override Result DoOpenFile(ref UniqueRef<IFile> outFile, ref readonly Path path, OpenMode mode)
        {
            if (IsPathNotShallow(path)) { return ResultFs.PathNotFound.Value; }
            return impl.OpenFile(ref outFile, in path, mode);
        }

        protected override Result DoOpenDirectory(ref UniqueRef<IDirectory> outDirectory, ref readonly Path path, OpenDirectoryMode mode)
        {
            if (IsPathNotShallow(path)) { return ResultFs.PathNotFound.Value; }
            var result = impl.OpenDirectory(ref outDirectory, in path, mode);
            if (result.IsFailure()) 
                return result;

            outDirectory.Reset(new Directory(outDirectory.Get));
            return result;
        }

        protected override Result DoCommit()
        {
            return impl.Commit();
        }

        public class Directory(IDirectory impl) : IDirectory
        {
            private int? RealCount;

            private Result TryGetCount(Span<DirectoryEntry> buffer)
            {
                var temp = new DirectoryEntry[buffer.Length];
                var result = impl.Read(out var count, temp);
                if (result.IsFailure()) return result;

                var realCount = 0;
                for (var i = 0; i < count; i++)
                {
                    var name = StringUtils.Utf8ZToString(temp[i].Name);
                    if (temp[i].Type != DirectoryEntryType.File)
                        continue;

                    var isNca = name.EndsWith(".nca");
                    var isTik = name.EndsWith(".tik");

                    if(!isNca && !isTik)
                        continue;

                    var isCnmt = isNca && name.EndsWith(".cnmt.nca");
                    /* 16 bytes for NCA ID, then 4 chars for ".nca" */
                    if (!isCnmt && name.Length != (16 * 2) + 4)
                        continue;

                    buffer[realCount] = temp[i];
                    realCount++;
                }

                RealCount = realCount;

                return result;
            }

            protected override Result DoRead(out long entriesRead, Span<DirectoryEntry> entryBuffer)
            {
                entriesRead = 0;
                var result = TryGetCount(entryBuffer);
                if (RealCount != null)
                    entriesRead = (long)RealCount;
                return result;
            }

            protected override Result DoGetEntryCount(out long entryCount)
            {
                entryCount = 0;

                if (RealCount != null)
                {
                    entryCount = (long)RealCount;
                    return Result.Success;
                }

                var buffer = new DirectoryEntry[100];
                var result = TryGetCount(buffer);

                if (RealCount != null)
                    entryCount = (long)RealCount;

                return result;
            }
        }
    }
}
