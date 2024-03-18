using LibHac.Tools.Fs;

namespace nxmount.Apps
{
    public class ApplicationInfo
    {
        internal Application Impl;
        internal List<TitleInfo> Updates = [];
        internal List<TitleInfo> Aoc = [];

        public TitleInfo? Base { get; internal init; }
        public TitleInfo? Latest { get; internal init; }

        public TitleInfo? LatestOrBase => Latest ?? Base;

        public ApplicationInfo(Application app)
        {
            Impl = app;
            if(app.Main != null)
                Base = new TitleInfo(app.Main, TitleInfo.TitleType.Base);

            foreach (var patch in Impl.Patches)
            {
                var patchTitle = new TitleInfo(patch, TitleInfo.TitleType.Patch);
                if (Base != null)
                {
                    foreach (var key in patchTitle.ContentKeys)
                    {
                        var patchNca = patchTitle.GetNcaByContentKey(key)!;
                        var baseNca = Base.GetNcaByContentKey(key);

                        if (baseNca == null)
                            continue;

                        patchNca.BaseNca = baseNca.Nca;
                    }
                }

                Updates.Add(patchTitle);

                if (Latest == null || Latest!.Impl.Version.Version < patch.Version.Version)
                    Latest = patchTitle;
            }

            foreach(var aoc in Impl.AddOnContent)
                Aoc.Add(new TitleInfo(aoc, TitleInfo.TitleType.Aoc));

        }

        public IEnumerable<TitleInfo> Versions
        {
            get
            {
                if (Base != null)
                    yield return Base;

                foreach (var update in Updates)
                    yield return update;
            }
        }
    }

}
