using LibHac.Tools.Fs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac;
using LibHac.Ncm;
using LibHac.Util;

namespace nxmount.Apps
{
    public class ApplicationInfo
    {
        internal Application Impl;
        internal List<TitleInfo> Updates = [];
        internal List<TitleInfo> Aoc = [];

        public TitleInfo? Base { get; internal init; }

        public string DisplayTitle => Impl.Main.Name;

        public ApplicationInfo(Application app)
        {
            Impl = app;
            if(app.Main != null)
                Base = new TitleInfo(app.Main, TitleInfo.TitleType.Base);

            foreach(var aoc in Impl.AddOnContent)
                Aoc.Add(new TitleInfo(aoc, TitleInfo.TitleType.Aoc));
        }

        public void AddUpdate(Title update)
        {
            Updates.Add(new TitleInfo(update, TitleInfo.TitleType.Patch));
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

        public override string ToString() => DisplayTitle;
    }

}
