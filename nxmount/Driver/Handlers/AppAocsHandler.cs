using System.Diagnostics;
using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using nxmount.Apps;
using nxmount.Driver.Interfaces;

namespace nxmount.Driver.Handlers
{
    public class AppAocsHandler : CachedFsNodeHandler
    {
        private const string MissingKey = " (Missing Title Key)";
        public override string FileName => "Aoc";

        private ApplicationInfo Info;

        public AppAocsHandler(IDirectoryHandler? parent, ApplicationInfo info) : base(parent)
        {
            Info = info;
        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            foreach (var aoc in Info.Aoc)
            {
                IFileInfo info;
                if (!aoc.HaveKeysForNca(new TitleInfo.ContentKey(ContentType.Data, 0)))
                {
                    info = context.CreateNewFileInfo(false);
                    info.FileName = aoc.DisplayVersion + MissingKey;
                }
                else
                {
                    info = context.CreateNewFileInfo(true);
                    info.FileName = aoc.DisplayVersion;
                }

                yield return info;
            }
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            var missingKey = name.EndsWith(MissingKey);
            if (missingKey)
                name = name[..^MissingKey.Length];

            var info = Info.Aoc.FirstOrDefault(x => x.DisplayVersion == name);
            if (info == null)
                return null;


            /* Ensure there is only a Data content type. */
            var types = info.ContentTypes.ToList();
            Debug.Assert(types.Count == 1 && types[0] == ContentType.Data);
            /* Ensure there is only one NCA for the Data content type. */
            Debug.Assert(info.GetContentIdsByType(ContentType.Data).Length == 1);

            var key = new TitleInfo.ContentKey(ContentType.Data, 0);
            var hasKey = info.HaveKeysForNca(key);

            /* Ensure there is only one Data section in the one Data NCA. */
            var sections = info.GetSections(key).ToList();
            Debug.Assert(!hasKey || sections.Count == 1 && sections[0] == NcaSectionType.Data);

            if (missingKey || !hasKey)
                return new DummyFileNode()
                {
                    FileName = info.DisplayVersion + MissingKey,
                    Parent = this,
                };

            return new SectionFilesystemHandler(this, info, key, NcaSectionType.Data);
        }
    }
}
