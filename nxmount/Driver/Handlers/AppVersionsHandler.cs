using System.Globalization;
using System.Text;
using System.Xml.Linq;
using LibHac.Tools.Fs;
using nxmount.Apps;
using nxmount.Driver.Interfaces;

namespace nxmount.Driver.Handlers
{
    public partial class AppVersionsHandler : CachedFsNodeHandler
    {
        private const string BaseSuffx = " (Base)";
        private const string TitleIdPrefix = "Title Id - ";

        private DummyFileNode TitleIdFile;
        private ulong Tid => (Info.Base ?? Info.Latest)!.Impl.Id;
        public override string FileName { get; }

        private ApplicationInfo Info;
        private AppAocsHandler? AocHandler;

        public AppVersionsHandler(IDirectoryHandler? parent, ApplicationInfo info, string filename) : base(parent)
        {
            Info = info;
            FileName = filename;

            if (info.Aoc.Count > 0)
            {
                AocHandler = new AppAocsHandler(this, info);
            }

            var tidString = $"{Tid:X16}";
            TitleIdFile = new DummyFileNode()
            {
                FileName = $"{TitleIdPrefix}{tidString}.txt",
                Parent = this,
                Data = Encoding.UTF8.GetBytes(tidString)
            };
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            if (name == TitleIdFile.FileName)
            {
                return TitleIdFile;
            }

            if (AocHandler != null && name == AocHandler.FileName)
            {
                return AocHandler;
            }

            var lookingForBase = name.EndsWith(BaseSuffx);
            if (lookingForBase)
                name = name[..^BaseSuffx.Length];

            /* Try to find the version info. */
            var info = Info.Versions.FirstOrDefault(x =>
            {
                var matching = true;
                matching &= x.DisplayVersion == name;
                matching &= lookingForBase == (x.Type != TitleInfo.TitleType.Patch);
                return matching;
            });
            
            if (info == null)
                return null;

            return new VersionContentTypesHandler(this, info);
        }

        public override IEnumerable<IFileInfo> QueryThisDirectory(Context context)
        {
            foreach (var version in Info.Versions)
            {
                var name = version.DisplayVersion;
                if (version.Type == TitleInfo.TitleType.Base)
                    name += BaseSuffx;

                var info = context.CreateNewFileInfo(true);
                info.FileName = FilterFileName(name);
                yield return info;
            }

            if (AocHandler != null)
            {
                AocHandler.QueryFileInfo(context, out var info);
                yield return info;
            }

            TitleIdFile.QueryFileInfo(context, out var tidInfo);
            yield return tidInfo;
        }
    }
}
