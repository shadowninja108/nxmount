namespace nxmount.Driver.Interfaces
{
    public interface IFsNodeHandler : IDisposable
    {
        public IDirectoryHandler? Parent { get;  }
        public string FileName { get; }
        bool QueryFileInfo(Context context, out IFileInfo info);
    }
}
