using FuseDotNet;
using FuseDotNet.Extensions;
using nxmount.Apps;
using nxmount.Driver;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Linux.Driver
{
    public class FuseDriver : DriverBase<PosixResult>, IFuseOperations
    {
        private PosixResult ENOSYS => PosixResult.ENOTSOCK;

        private class FuseDriverFileInfo : IFileInfo
        {
            private string _fileName;
            private PosixOpenFlags _attributes;

            public int FileNameHash { get; private set; }

            public string FileName
            {
                get => _fileName;
                set
                {
                    FileNameHash = value.GetHashCode();
                    _fileName = value;
                }
            }

            public FileAttributes Attributes
            {
                get
                {
                    var attr = FileAttributes.None;
                    if((_attributes & PosixOpenFlags.Directory) == PosixOpenFlags.Directory)
                        attr |= FileAttributes.Directory;
                    return attr;
                }
                set
                {
                    var attr = PosixOpenFlags.Read;
                    if ((value & FileAttributes.Directory) == FileAttributes.Directory)
                        attr |= PosixOpenFlags.Directory;
                    _attributes = attr;
                }
            }
            public long Length { get; set; }

            public FuseDirEntry ToDirEntry()
            {
                PopulateStat(out var stat);
                return new()
                {
                    Name = FileName,

                    /* TODO? */
                    Flags = 0,
                    Offset = 0,

                    Stat = stat
                };
            }

            public void PopulateStat(out FuseFileStat stat)
            {
                var now = TimeSpec.Now();
                Console.WriteLine($"Populate {Attributes}");
                var attributes = Attributes.ToPosixFileMode();
                attributes &= ~(
                    PosixFileMode.GroupWrite |
                    PosixFileMode.OthersWrite |
                    PosixFileMode.OwnerWrite |
                    PosixFileMode.GroupExecute |
                    PosixFileMode.OthersExecute |
                    PosixFileMode.OwnerExecute
                );

                if ((Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    stat = new()
                    {
                        st_birthtim = now,
                        st_mtim = now,
                        st_ctim = now,
                        st_atim = now,
                        st_mode = attributes | PosixFileMode.Directory,
                    };
                }
                else
                {
                    stat = new()
                    {
                        st_size = Length,
                        st_birthtim = now,
                        st_mtim = now,
                        st_ctim = now,
                        st_atim = now,
                        st_mode = attributes,
                    };
                }
            }
        }

        private class FuseContext(AppManager manager, ApplicationLanguage desiredLanguage) : Context(manager, desiredLanguage)
        {
            public override IFileInfo CreateNewFileInfo(bool isDirectory = false)
            {
                IFileInfo fi =  new FuseDriverFileInfo();
                FileInfoUtils.PopulateFileInfo(ref fi, isDirectory);
                return fi;
            }
        }

        public override PosixResult ResultSuccess => PosixResult.Success;
        public override PosixResult ResultFileNotFound => PosixResult.ENOENT;
        public override PosixResult ResultPathNotFound => PosixResult.ENOENT;
        public override PosixResult ResultNotImplemented => ENOSYS;

        public FuseDriver(AppManager manager, ApplicationLanguage desiredLanguage) : base(new FuseContext(manager, desiredLanguage))
        {
        }

        public string GetPath(ReadOnlyFuseMemory<byte> fileNamePtr) => FuseHelper.GetStringFromSpan(fileNamePtr.Span);

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public PosixResult OpenDir(ReadOnlyFuseMemory<byte> fileNamePtr, ref FuseFileInfo fileInfo)
        {
            Console.WriteLine("OpenDir");

            var result = TryGetHandler(out _, GetPath(fileNamePtr), out var handler);
            if (result != ResultSuccess)
            {
                Console.WriteLine($"Fail: {result}");
                return result;
            }
            if (handler is not IDirectoryHandler)
            {
                return PosixResult.ENOENT;
            }

            fileInfo.Context = handler;
            return PosixResult.Success;
        }

        public PosixResult GetAttr(ReadOnlyFuseMemory<byte> fileNamePtr, out FuseFileStat stat, ref FuseFileInfo fileInfo)
        {
            stat = default;
            try
            {
                Console.WriteLine("GetAttr");

                var result = TryGetHandler(out _, GetPath(fileNamePtr), out var handler);
                if (result != ResultSuccess)
                {
                    Console.WriteLine($"Fail: {result}");
                    return result;
                }

                if (!handler!.QueryFileInfo(Context, out var fi))
                {
                    Console.WriteLine($"Fail: {PosixResult.EACCES}");
                    return PosixResult.EACCES;
                }

                ((FuseDriverFileInfo)fi).PopulateStat(out stat);

                // Console.WriteLine($@"
                //     st_size = {stat.st_size}
                //     st_nlink = {stat.st_nlink}
                //     st_mode = {stat.st_mode}
                //     st_gen = {stat.st_gen}
                //     st_birthtim = {stat.st_birthtim}
                //     st_atim = {stat.st_atim}
                //     st_ctim = {stat.st_ctim}
                //     st_mtim = {stat.st_mtim}
                //     st_ino = {stat.st_ino}
                //     st_dev = {stat.st_dev}
                //     st_rdev = {stat.st_rdev}
                //     st_uid = {stat.st_uid}
                //     st_gid = {stat.st_gid}
                //     st_blksize = {stat.st_blksize}
                //     st_blocks = {stat.st_blocks}
                // ");
                return PosixResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return PosixResult.EREMOTE;
            }
        }

        public PosixResult Read(ReadOnlyFuseMemory<byte> fileNamePtr, FuseMemory<byte> buffer, long position, out int readLength,
            ref FuseFileInfo fileInfo)
        {
            Console.WriteLine($"Read {position}+{buffer.Length}");
            readLength = 0;
            var handler = (IFsNodeHandler) fileInfo.Context;
            if (handler is not IFileNodeHandler file)
            {
                Console.WriteLine($"Fail: {PosixResult.EBADF}");
                return PosixResult.EBADF;
            }

            if (position > file.Size)
            {
                Console.WriteLine($"Fail: {PosixResult.EBADF}");
                return PosixResult.EBADF;
            }

            if (!file.Read(buffer.Span, out readLength, position))
            {
                Console.WriteLine($"Fail: {PosixResult.EREMOTE}");
                return PosixResult.EREMOTE;
            }

            return ResultSuccess;
        }

        public PosixResult ReadDir(ReadOnlyFuseMemory<byte> fileNamePtr, out IEnumerable<FuseDirEntry> entries, ref FuseFileInfo fileInfo, long offset,
            FuseReadDirFlags flags)
        {
            Console.WriteLine("ReadDir");
            entries = [];
            var handler = (IFsNodeHandler)fileInfo.Context;
            if (handler is not IDirectoryHandler dir)
                return PosixResult.ENFILE;

            entries = dir.QueryChildren(Context, ReadOnlySpan<string>.Empty)
                .Cast<FuseDriverFileInfo>().Select(x => x.ToDirEntry());

            return PosixResult.Success;
        }

        public PosixResult Open(ReadOnlyFuseMemory<byte> fileNamePtr, ref FuseFileInfo fileInfo)
        {
            Console.WriteLine("Open");
            var result = TryGetHandler(out _, GetPath(fileNamePtr), out var handler);
            if (result != ResultSuccess)
                return result;

            fileInfo.Context = handler;
            return PosixResult.Success;
        }

        public PosixResult Access(ReadOnlyFuseMemory<byte> fileNamePtr, PosixAccessMode mask)
        {
            Console.WriteLine("Access");
            var result = TryGetHandler(out _, GetPath(fileNamePtr), out var handler);
            if (result != ResultSuccess)
                return PosixResult.ENOENT;
            if(handler == null)
                return PosixResult.ENOENT;

            return result;
        }

        public PosixResult ReleaseDir(ReadOnlyFuseMemory<byte> fileNamePtr, ref FuseFileInfo fileInfo)
        {
            Console.WriteLine("ReleaseDir");
            /* TODO: */
            fileInfo.Context = null;
            return PosixResult.Success;
        }

        public PosixResult Release(ReadOnlyFuseMemory<byte> fileNamePtr, ref FuseFileInfo fileInfo)
        {
            Console.WriteLine("Release");
            /* TODO: */
            fileInfo.Context = null;
            return PosixResult.Success;
        }

        public PosixResult StatFs(ReadOnlyFuseMemory<byte> fileNamePtr, out FuseVfsStat statvfs)
        {
            Console.WriteLine("StatFs");
            statvfs = default;
            statvfs.f_flag |= 1; /* ST_RDONLY */
            return PosixResult.Success;
        }

        public void Init(ref FuseConnInfo fuse_conn_info) { }

        public PosixResult Write(ReadOnlyFuseMemory<byte> fileNamePtr, ReadOnlyFuseMemory<byte> buffer, long position, out int writtenLength,
            ref FuseFileInfo fileInfo)
        {
            writtenLength = 0;
            return PosixResult.EROFS;
        }

        public PosixResult FSyncDir(ReadOnlyFuseMemory<byte> fileNamePtr, bool datasync, ref FuseFileInfo fileInfo) => ENOSYS;

        public PosixResult ReadLink(ReadOnlyFuseMemory<byte> fileNamePtr, FuseMemory<byte> target) => ENOSYS;

        public PosixResult Link(ReadOnlyFuseMemory<byte> from, ReadOnlyFuseMemory<byte> to) => PosixResult.EROFS;

        public PosixResult MkDir(ReadOnlyFuseMemory<byte> fileNamePtr, PosixFileMode mode) => PosixResult.EROFS;

        public PosixResult RmDir(ReadOnlyFuseMemory<byte> fileNamePtr) => PosixResult.EROFS;

        public PosixResult FSync(ReadOnlyFuseMemory<byte> fileNamePtr, bool datasync, ref FuseFileInfo fileInfo) => ENOSYS;

        public PosixResult Unlink(ReadOnlyFuseMemory<byte> fileNamePtr) => PosixResult.EROFS;

        public PosixResult SymLink(ReadOnlyFuseMemory<byte> from, ReadOnlyFuseMemory<byte> to) => PosixResult.EROFS;

        public PosixResult Flush(ReadOnlyFuseMemory<byte> fileNamePtr, ref FuseFileInfo fileInfo) => ENOSYS;

        public PosixResult Rename(ReadOnlyFuseMemory<byte> from, ReadOnlyFuseMemory<byte> to) => PosixResult.EROFS;

        public PosixResult Truncate(ReadOnlyFuseMemory<byte> fileNamePtr, long size) => PosixResult.EROFS;

        public PosixResult UTime(ReadOnlyFuseMemory<byte> fileNamePtr, TimeSpec atime, TimeSpec mtime, ref FuseFileInfo fileInfo) => PosixResult.EROFS;

        public PosixResult Create(ReadOnlyFuseMemory<byte> fileNamePtr, PosixFileMode mode, ref FuseFileInfo fileInfo) => ENOSYS;

        public PosixResult IoCtl(ReadOnlyFuseMemory<byte> fileNamePtr, int cmd, IntPtr arg, ref FuseFileInfo fileInfo, FuseIoctlFlags flags, IntPtr data) => ENOSYS;
    }
}
