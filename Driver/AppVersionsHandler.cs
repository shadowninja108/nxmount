using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;
using LibHac.Gc.Impl;
using nxmount.Apps;
using nxmount.Driver.Interfaces;
using nxmount.Util;

namespace nxmount.Driver
{
    public class AppVersionsHandler : CachedFsNodeHandler
    {
        private const string BaseSuffx = " (Base)";

        public override string FileName => Info.DisplayTitle;

        private ApplicationInfo Info;
        private AppAocsHandler? AocHandler;


        public AppVersionsHandler(IFsNodeHandler? parent, ApplicationInfo info) : base(parent)
        {
            Info = info;

            if (info.Aoc.Count > 0)
            {
                AocHandler = new AppAocsHandler(this, info);
            }
        }

        public override IFsNodeHandler? GetNodeHandlerImpl(string name)
        {
            if (AocHandler != null && name == AocHandler.FileName)
            {
                return AocHandler;
            }

            var lookingForBase = name.EndsWith(BaseSuffx);
            if(lookingForBase)
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

            return new VersionContentsHandler(this, info);
        }

        public override bool QueryThisDirectory(Context context, ref IList<FileInformation> files)
        {
            foreach (var version in Info.Versions)
            {
                var name = version.DisplayVersion;
                if (version.Type == TitleInfo.TitleType.Base)
                    name += BaseSuffx;

                var info = new FileInformation()
                {
                    FileName = FilterFileName(name)
                };
                info.PopulateFileInfo(true);
                files.Add(info);
            }

            if (Info.Aoc.Count > 0)
            {
                var info = DokanUtils.CreateFileInfo(true);
                info.FileName = "AOC";
                files.Add(info);
            }

            return true;
        }
    }
}
