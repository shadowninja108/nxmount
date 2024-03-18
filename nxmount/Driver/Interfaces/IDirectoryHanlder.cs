namespace nxmount.Driver.Interfaces
{
    public interface IDirectoryHandler : IFsNodeHandler
    {
        IEnumerable<IFileInfo> QueryChildren(Context context, ReadOnlySpan<string> pathComponents);
        bool QueryNode(Context context, ReadOnlySpan<string> pathComponents, out IFsNodeHandler? info);
        void DisposeChild(IFsNodeHandler handlerToDispose);
    }
}
