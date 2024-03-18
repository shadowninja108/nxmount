using LibHac.Tools.FsSystem.NcaUtils;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;
using ContentType = LibHac.Ncm.ContentType;

namespace nxmount.Driver.Handlers
{
    internal class VersionDeltasHandler : CachedFsNodeHandler
    {
        private const string MissingKey = " (Missing Title Key)";
        public override string FileName
        {
            get
            {
                var name = FilterFileName(ContentType.DeltaFragment.ToString());
                if (Info.DeltaNcas.Count > 1)
                    name += "s";

                return name;
            }
        }
        private TitleInfo Info;

        public VersionDeltasHandler(IDirectoryHandler? parent, TitleInfo info) : base(parent)
        {
            Info = info;
        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            var deltas = Info.Impl.Metadata.ExtendedData.DeltaContents;
            for (var i = 0; i < deltas.Length; i++)
            {
                var id = deltas[i].NcaId;
                var nca = Info.GetNcaById(id);
                if (nca == null)
                    continue;

                IFileInfo info;
                if (!nca.HasTitleKey())
                {
                    info = context.CreateNewFileInfo(false);
                    info.FileName = i + MissingKey;
                }
                else
                {
                    info = context.CreateNewFileInfo(true);
                    info.FileName = i.ToString();
                }

                yield return info;
            }
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            var missingKey = name.EndsWith(MissingKey);
            if (missingKey)
                name = name[..^MissingKey.Length];

            var deltas = Info.Impl.Metadata.ExtendedData.DeltaContents;

            if (!byte.TryParse(name, out var index))
                return null;

            if (index >= deltas.Length)
                return null;

            var key = new TitleInfo.ContentKey(ContentType.DeltaFragment, index);
            if (missingKey || !Info.HaveKeysForNca(key))
                return new DummyFileNode()
                {
                    FileName = name + MissingKey,
                    Parent = this
                };

            return new SectionFilesystemHandler(this, Info, key, NcaSectionType.Data);
        }
    }
}
