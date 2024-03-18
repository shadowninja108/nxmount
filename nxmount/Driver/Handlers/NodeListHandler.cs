using nxmount.Driver.Interfaces;

namespace nxmount.Driver.Handlers
{
    public class NodeListHandler : CachedFsNodeHandler
    {
        public override string FileName => "/";

        private List<IFsNodeHandler>? Handlers;

        public NodeListHandler(IDirectoryHandler? parent) : base(parent)
        {
        }

        public void Initialize(List<IFsNodeHandler> handlers)
        {
            Handlers = handlers;
        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            foreach (var handler in Handlers!)
            {
                if (!handler.QueryFileInfo(context, out var info))
                {
                    throw new QueryFailedException();
                }

                yield return info;
            }
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name) => Handlers!.FirstOrDefault(x => name == x.FileName);

    }
}
