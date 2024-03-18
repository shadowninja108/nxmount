using LibHac.Tools.FsSystem.NcaUtils;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using static nxmount.Apps.TitleInfo;

namespace nxmount.Driver.Handlers
{
    public class ContentSectionsHandler : CachedFsNodeHandler
    {
        private const string MissingBase = " (Missing Base)";
        public override string FileName => ContentKey.Type.ToString();

        private readonly TitleInfo Info;
        private readonly ContentKey ContentKey;

        public ContentSectionsHandler(IDirectoryHandler? parent, TitleInfo info, ContentKey key) : base(parent)
        {
            Info = info;
            ContentKey = key;
        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            foreach (var section in Info.GetSections(ContentKey))
            {
                var name = FilterFileName(section.ToString());
                IFileInfo info;
                if (Info.IsSectionMissingBase(ContentKey, section))
                {
                    info = context.CreateNewFileInfo(false);
                    info.FileName = name + MissingBase;
                }
                else
                {
                    info = context.CreateNewFileInfo(true);
                    info.FileName = name;
                }
                yield return info;
            }
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            var missingBase = name.EndsWith(MissingBase);
            if (missingBase)
                name = name[..^MissingBase.Length];

            if (!Enum.TryParse<NcaSectionType>(name, out var type))
                return null;

            if (!Info.GetSections(ContentKey).Contains(type))
                return null;

            if (Info.IsSectionMissingBase(ContentKey, type))
                return new DummyFileNode()
                {
                    FileName = name,
                    Parent = this
                };

            return new SectionFilesystemHandler(this, Info, ContentKey, type);
        }
    }
}
