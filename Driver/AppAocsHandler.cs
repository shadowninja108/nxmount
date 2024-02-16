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
    public class AppAocsHandler : CachedFsNodeHandler
    {
        public override string FileName => "Aoc";

        private ApplicationInfo Info;

        public AppAocsHandler(IFsNodeHandler? parent, ApplicationInfo info) : base(parent)
        {
            Info = info;
        }

        public override bool QueryThisDirectory(Context context, ref IList<FileInformation> files)
        {
            foreach (var aoc in Info.Aoc)
            {
                var info = DokanUtils.CreateFileInfo(true);
                info.FileName = aoc.DisplayVersion;
                files.Add(info);
            }

            return true;
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            throw new NotImplementedException();
        }
    }
}
