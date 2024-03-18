using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver.Handlers
{
    public class GamesHandler : CachedFsNodeHandler
    {
        public override string FileName => "Games";

        private Dictionary<string, AppVersionsHandler> Handlers = new();

        public GamesHandler(IDirectoryHandler? parent, Context context) : base(parent)
        {
            foreach (var (_, info) in context.Manager.Applications)
            {
                if (info.LatestOrBase == null)
                    continue;

                var title = PathUtils.CleanupFilename(info.LatestOrBase.GetTitle(context.DesiredLanguage));
                Handlers[title] = new AppVersionsHandler(this, info, title);
            }
        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            foreach (var (_, handler) in Handlers)
            {
                if (!handler.QueryFileInfo(context, out var info))
                    yield break;

                yield return info;
            }
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name) => Handlers.GetValueOrDefault(name);
    }
}
