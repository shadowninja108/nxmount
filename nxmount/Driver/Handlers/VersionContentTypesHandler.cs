using System.Diagnostics;
using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using nxmount.Apps;
using nxmount.Driver.Interfaces;

namespace nxmount.Driver.Handlers
{
    public class VersionContentTypesHandler : CachedFsNodeHandler
    {
        private const string MissingBase = " (Missing Base)";
        private const string MissingKey = " (Missing Title Key)";
        public override string FileName => Info.DisplayVersion;

        private readonly TitleInfo Info;
        private readonly VersionDeltasHandler? DeltasHandler;

        public VersionContentTypesHandler(IDirectoryHandler? parent, TitleInfo info) : base(parent)
        {
            Info = info;

            if (!Info.DeltaNcas.IsEmpty)
                DeltasHandler = new VersionDeltasHandler(this, info);
        }

        private static bool TryRemoveSuffix(ref string str, string suffix)
        {
            var didHaveSuffix = str.EndsWith(suffix);
            if (didHaveSuffix)
                str = str[..^suffix.Length];

            return didHaveSuffix;
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            var missingKey = TryRemoveSuffix(ref name, MissingKey);
            var missingBase = TryRemoveSuffix(ref name, MissingBase);

            if (missingBase != missingKey)
            {
                if (missingBase)
                    missingKey = TryRemoveSuffix(ref name, MissingKey);
                else
                    missingBase = TryRemoveSuffix(ref name, MissingBase);
            }

            if (!Enum.TryParse<ContentType>(name, out var type))
            {
                if (!name.EndsWith('s'))
                    return null;

                if (!Enum.TryParse(name[..^1], out type))
                    return null;
            }

            if (!Info.ContentTypes.Contains(type))
                return null;

            if (type == ContentType.DeltaFragment)
            {
                return DeltasHandler;
            }

            var offsets = Info.GetContentIdsByType(type);
            /* If there's only one ContentId, we just skip to its contents.*/
            if (offsets.Length == 1)
            {
                var key = new TitleInfo.ContentKey(type, 0);

                if (missingKey || !Info.HaveKeysForNca(key))
                    return new DummyFileNode()
                    {
                        FileName = name + MissingKey,
                        Parent = this
                    };

                var sections = Info.GetSections(key).ToList();

                /* Handle case where there is only one section in the NCA. */
                if (sections.Count == 1)
                {
                    if (missingBase || Info.IsSectionMissingBase(key, sections[0]))
                        return new DummyFileNode()
                        {
                            FileName = name + MissingBase,
                            Parent = this,
                        };

                    /* If the given NCA only has one section and it's Data, just skip to using that. */
                    if (sections[0] == NcaSectionType.Data)
                        return new SectionFilesystemHandler(this, Info, key, NcaSectionType.Data);
                }

                /* Provide selection of what section to use. */
                return new ContentSectionsHandler(this, Info, key);
            }
            if (offsets.Length > 1)
            {
                /* Provide list of content ids. */
                return new ContentOffsetsHandler(this, Info, type);
            }

            return null;

        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            foreach (var type in Info.ContentTypes)
            {
                var name = FilterFileName(type.ToString());
                var offsets = Info.GetContentIdsByType(type);

                IFileInfo info;
                if (offsets.Length <= 1)
                {
                    Debug.Assert(offsets.Length == 1);

                    var key = new TitleInfo.ContentKey(type, 0);
                    var sections = Info.GetSections(key).ToList();
                    if (!Info.HaveKeysForNca(key))
                    {
                        info = context.CreateNewFileInfo(false);
                        info.FileName = name + MissingKey;
                    }
                    else if (sections.Count == 1 && sections[0] == NcaSectionType.Data && Info.IsSectionMissingBase(key, NcaSectionType.Data))
                    {
                        info = context.CreateNewFileInfo(false);
                        info.FileName = name + MissingBase;
                    }
                    else
                    {
                        info = context.CreateNewFileInfo(true);
                        info.FileName = name;
                    }
                }
                else
                {
                    /* When there are multiple of this kind of content type. */
                    info = context.CreateNewFileInfo(true);
                    info.FileName = name + "s";
                }

                yield return info;
            }
        }
    }
}
