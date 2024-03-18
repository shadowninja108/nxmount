using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver.Handlers;

public class DummyFileNode : IFileNodeHandler
{
    public IDirectoryHandler? Parent { get; init; }
    public required string FileName { get; init; }

    public byte[]? Data = null;

    public bool QueryFileInfo(Context context, out IFileInfo info)
    {
        info = context.CreateNewFileInfo(false);
        info.FileName = PathUtils.CleanupFilename(FileName);
        info.Length = Size;
        return true;
    }

    public long Size => Data?.Length ?? 0;
    public bool Read(Span<byte> buffer, out int bytesRead, long offset)
    {
        bytesRead = 0;
        if(offset < 0)
            return false;

        if (Data == null)
        {
            if (offset != 0 || buffer.Length != 0)
                return false;
        }
        else
        {
            /* Ensure the start is within bounds. */
            if(Data.Length <= offset)
                return false;

            var length = (int) Math.Min(buffer.Length, Data.Length - offset);
            /* Do the read. */
            Data.AsSpan((int)offset, length).CopyTo(buffer);
            bytesRead = length;
        }

        return true;
    }

    public void Dispose() { }
}