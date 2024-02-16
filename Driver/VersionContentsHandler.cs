using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using LibHac.Ncm;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver
{
    public class VersionContentsHandler : CachedFsNodeHandler
    {
        public override string FileName => Info.DisplayVersion;

        private readonly TitleInfo Info;

        public VersionContentsHandler(IFsNodeHandler? parent, TitleInfo info) : base(parent)
        {
            Info = info;
        }

        public override SectionsHandler? GetNodeHandlerImpl(string name)
        {
            if (!Enum.TryParse<ContentType>(name, out var type))
                return null;

            if(!Info.ContentTypes.Contains(type))
                return null;

            return new SectionsHandler(this, Info, type);
        }

        public override bool QueryThisDirectory(Context context, ref IList<FileInformation> files)
        {
            foreach (var type in Info.ContentTypes)
            {
                var info = DokanUtils.CreateFileInfo(true);
                info.FileName = FilterFileName(type.ToString());
                files.Add(info);
            }

            return true;
        }
    }
}
