using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver
{
    public class SectionsHandler : CachedFsNodeHandler
    {
        public override string FileName => ContentType.ToString();

        private readonly TitleInfo Info;
        private readonly ContentType ContentType;

        public SectionsHandler(IFsNodeHandler? parent, TitleInfo info, ContentType type) : base(parent)
        {
            Info = info;
            ContentType = type;
        }

        public override bool QueryThisDirectory(Context context, ref IList<FileInformation> files)
        {
            foreach (var section in Info.GetSections(ContentType))
            {
                var info = DokanUtils.CreateFileInfo(true);
                info.FileName = FilterFileName(section.ToString());
                files.Add(info);
            }
            return true;
        }

        public override SectionContentHandler? GetNodeHandlerImpl(string name)
        {
            if (!Enum.TryParse<NcaSectionType>(name, out var type))
                return null;

            return new SectionContentHandler(this, Info, ContentType, type);
        }
    }
}
