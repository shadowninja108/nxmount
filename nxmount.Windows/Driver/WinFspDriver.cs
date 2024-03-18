using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Fsp.Interop;
using nxmount.Apps;
using nxmount.Driver;
using nxmount.Driver.Interfaces;
using nxmount.Util;
using FileInfo = Fsp.Interop.FileInfo;

namespace nxmount.Windows.Driver
{
    public class WinFspDriver : Fsp.FileSystemBase
    {
        private struct WinFspFileInfo : IFileInfo
        {
            public int FileNameHash { get; private set; }
            private string _fileName;

            public string FileName
            {
                readonly get => _fileName;
                set
                {
                    FileNameHash = value.GetHashCode();
                    _fileName = value;
                }
            }
            public FileAttributes Attributes { get; set; }
            public long Length { get; set; }


            public void Export(out FileInfo info)
            {
                info = new FileInfo
                {
                    FileAttributes = (uint)Attributes,
                    FileSize = (ulong)Length
                };
                /* TODO: time stuff */
            }
        }

        private class WinFspContext(AppManager manager, ApplicationLanguage desiredLanguage) : Context(manager, desiredLanguage)
        {
            public override IFileInfo CreateNewFileInfo(bool isDirectory = false)
            {
                IFileInfo info = new WinFspFileInfo();
                FileInfoUtils.PopulateFileInfo(ref info, isDirectory);
                return info;
            }
        }


        private class DriverImpl : DriverBase<int>
        {
            public DriverImpl(AppManager manager, ApplicationLanguage desiredLanguage) : base(new WinFspContext(manager, desiredLanguage)) { }

            public override int ResultSuccess => STATUS_SUCCESS;

            public override int ResultFileNotFound => STATUS_OBJECT_NAME_NOT_FOUND;

            public override int ResultPathNotFound => STATUS_OBJECT_PATH_NOT_FOUND;

            public override int ResultNotImplemented => STATUS_NOT_IMPLEMENTED;
        }

        private DriverImpl Driver;

        public WinFspDriver(AppManager manager, ApplicationLanguage desiredLanguage)
        {
            Driver = new DriverImpl(manager, desiredLanguage);
        }

        public override int GetSecurityByName(
            string FileName,
            out uint FileAttributes,
            ref byte[] SecurityDescriptor
            )
        {
            FileAttributes = 0;
            SecurityDescriptor = null;

            var isDirectory = false;
            var isCreateNew = false;
            if (isDirectory && isCreateNew)
                return STATUS_ACCESS_DENIED;

            var result = Driver.TryGetHandler(out var split, FileName, out var handler);
            if (result != Driver.ResultSuccess)
                return result;

            if (handler is IDirectoryHandler)
                FileAttributes |= (uint)System.IO.FileAttributes.Directory;

            return Driver.ResultSuccess;
        }

        public override int Open(
            string FileName,
            uint CreateOptions,
            uint GrantedAccess,
            out object FileNode,
            out object FileDesc,
            out FileInfo FileInfo,
            out string NormalizedName
        )
        {
            FileNode = default;
            FileDesc = default;
            FileInfo = default;
            NormalizedName = default;

            var isDirectory = false;
            var isCreateNew = false;
            if (isDirectory && isCreateNew)
                return STATUS_ACCESS_DENIED;

            var result = Driver.TryGetHandler(out var split, FileName, out var handler);
            if (result != Driver.ResultSuccess)
                return result;

            FileNode = handler;

            if (!handler.QueryFileInfo(Driver.Context, out var fi))
                return STATUS_DATA_ERROR;
            if (fi is not WinFspFileInfo wfi)
                return STATUS_DATA_ERROR;

            wfi.Export(out FileInfo);

            NormalizedName = string.Join('/', split.ToArray());

            return Driver.ResultSuccess;
        }

        public override bool ReadDirectoryEntry(
            object FileNode,
            object FileDesc,
            string Pattern,
            string? Marker,
            ref object Context,
            out string FileName,
            out FileInfo FileInfo
        )
        {
            FileName = default;
            FileInfo = default;

            var handler = (IDirectoryHandler)FileNode;
            var enumerator = Context as IEnumerator<IFileInfo>;
            if (enumerator == null)
            {
                var enumerable = handler.QueryChildren(Driver.Context, ReadOnlySpan<string>.Empty);
                /* TODO: . and .. ? */
                Context = enumerator = enumerable.GetEnumerator();

                /* Do we need to skip forward to the marker? */
                if (Marker != null)
                {
                    var markerHash = Marker.GetHashCode();
                    /* Skip forward to the item after the marker. */
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.FileNameHash == markerHash)
                            break;
                    }
                }
            }

            while (enumerator.MoveNext())
            {
                try
                {
                    var info = (WinFspFileInfo)enumerator.Current;
                    FileName = info.FileName;
                    info.Export(out FileInfo);
                    return true;
                }
                catch (QueryFailedException e)
                {
                    /* Fallthrough as the query failed. */
                }
            }
            enumerator.Dispose();

            return false;
        }

        public override int GetVolumeInfo(out VolumeInfo VolumeInfo)
        {
            VolumeInfo = default;
            VolumeInfo.SetVolumeLabel("nxmount");

            return STATUS_SUCCESS;
        }

        public override int GetFileInfo(
            object FileNode,
            object FileDesc,
            out FileInfo FileInfo
        )
        {
            FileInfo = default;

            var handler = (IFsNodeHandler)FileNode;
            if (!handler.QueryFileInfo(Driver.Context, out var fi))
                return STATUS_DATA_ERROR;
            if (fi is not WinFspFileInfo dfi)
                return STATUS_DATA_ERROR;

            dfi.Export(out FileInfo);
            return Driver.ResultSuccess;
        }

        public override int Read(
            object FileNode,
            object FileDesc,
            nint Buffer,
            ulong Offset,
            uint Length,
            out uint BytesTransferred)
        {
            BytesTransferred = 0;
            var handler = (IFileNodeHandler)FileNode;

            if (Offset >= (ulong)handler.Size)
            {
                return STATUS_END_OF_FILE;
            }

            var endOffset = Offset + Length;
            if (endOffset > (ulong)handler.Size)
                endOffset = (ulong)handler.Size;

            var length = endOffset - Offset;
            var buffer = new byte[length];
            if (!handler.Read(buffer, out var bytesTransferred, (long)Offset))
                return STATUS_DATA_ERROR;

            Marshal.Copy(buffer, 0, Buffer, bytesTransferred);
            BytesTransferred = (uint)bytesTransferred;
            return Driver.ResultSuccess;
        }

        public override void Close(object FileNode, object FileDesc)
        {
            /* TODO: */
        }
    }
}
