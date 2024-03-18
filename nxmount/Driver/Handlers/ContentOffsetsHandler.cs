using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using nxmount.Apps;
using nxmount.Driver.Interfaces;

namespace nxmount.Driver.Handlers
{
    internal class ContentOffsetsHandler : CachedFsNodeHandler
    {
        private const string MissingKey = " (Missing Title Key)";
        private const string MissingBase = " (Missing Base)";
        public override string FileName => ContentType + "s";

        private TitleInfo Info;
        private ContentType ContentType;

        public ContentOffsetsHandler(IDirectoryHandler? parent, TitleInfo info, ContentType type) : base(parent)
        {
            Info = info;
            ContentType = type;
        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            var offsets = Info.GetContentIdsByType(ContentType);

            foreach (var offset in offsets)
            {
                IFileInfo info;
                var key = new TitleInfo.ContentKey(ContentType, offset);
                var sections = Info.GetSections(key).ToList();
                if (!Info.HaveKeysForNca(key))
                {
                    info = context.CreateNewFileInfo(false);
                    info.FileName = offset + MissingKey;
                }
                else if (sections.Count == 1 && sections[0] == NcaSectionType.Data && Info.IsSectionMissingBase(key, NcaSectionType.Data))
                {
                    info = context.CreateNewFileInfo(false);
                    info.FileName = offset + MissingBase;
                }
                else
                {
                    info = context.CreateNewFileInfo(true);
                    info.FileName = offset.ToString();
                }

                yield return info;
            }
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            var missingKey = name.EndsWith(MissingKey);
            if (missingKey)
                name = name[..^MissingKey.Length];

            if (!byte.TryParse(name, out var offset))
                return null;

            var offsets = Info.GetContentIdsByType(ContentType);
            if (!offsets.Contains(offset))
                return null;

            var key = new TitleInfo.ContentKey(ContentType, offset);
            var sections = Info.GetSections(key).ToList();

            if (missingKey || !Info.HaveKeysForNca(key))
                return new DummyFileNode
                {
                    FileName = name + MissingKey,
                    Parent = this
                };

            /* If the given NCA only has one section and it's Data, just skip to using that. */
            if (sections.Count == 1 && sections[0] == NcaSectionType.Data)
            {
                if (Info.IsSectionMissingBase(key, NcaSectionType.Data))
                    return new DummyFileNode()
                    {
                        FileName = name + MissingBase,
                        Parent = this
                    };

                return new SectionFilesystemHandler(this, Info, key, NcaSectionType.Data);
            }

            /* Provide selection of what section to use. */
            return new ContentSectionsHandler(this, Info, key);
        }
    }
}
