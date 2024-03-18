namespace nxmount.Driver.Interfaces
{
    public interface IFileNodeHandler : IFsNodeHandler
    {
        long Size { get; }

        bool Read(Span<byte> buffer, out int bytesRead, long offset);
    }
}
