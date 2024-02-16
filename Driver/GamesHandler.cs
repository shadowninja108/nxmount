using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver
{
    public class GamesHandler : CachedFsNodeHandler
    {
        public override string FileName => "Games";

        private Dictionary<string, AppVersionsHandler> Handlers = new();

        public GamesHandler(IFsNodeHandler? parent, Context context) : base(parent)
        {
            foreach (var (_, info) in context.Manager.Applications)
            {
                if (info.Base == null)
                    continue;

                var title = PathUtils.CleanupFilename(info.Base.GetTitle(context.DesiredLanguage));
                Handlers[title] = new AppVersionsHandler(this, info);
            }
        }

        public override bool QueryThisDirectory(Context context, ref IList<FileInformation> files)
        {
            foreach (var (_, handler) in Handlers)
            {
                var info = new FileInformation();
                if (!handler.QueryFileInfo(context, ref info))
                    return false;
                files.Add(info);
            }
            return true;
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name) => Handlers.GetValueOrDefault(name);
    }
}
