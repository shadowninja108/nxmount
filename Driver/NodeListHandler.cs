using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver
{
    public class NodeListHandler : CachedFsNodeHandler
    {
        public override string FileName => "/";

        private List<IFsNodeHandler>? Handlers;

        public NodeListHandler(IFsNodeHandler? parent) : base(parent)
        {
        }

        public void Initialize(List<IFsNodeHandler> handlers)
        {
            Handlers = handlers;
        }

        public override bool QueryThisDirectory(Context context, ref IList<FileInformation> files)
        {
            foreach (var handler in Handlers!)
            {
                var info = new FileInformation();
                if (!handler.QueryFileInfo(context, ref info))
                {
                    return false;
                }
                files.Add(info);
            }
            return true;
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name) => Handlers!.FirstOrDefault(x => name == x.FileName);

    }
}
