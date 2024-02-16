using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Fs.Fsa;
using LibHac.Ncm;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Util;
using nxmount.Apps.AppSource;
using nxmount.Util;

namespace nxmount.Apps
{
    public class AppManager : IDisposable
    {

        private readonly List<IFileSystem> Sources;
        private readonly IFileSystem Layered;
        private readonly SwitchFs Merged;
        public Dictionary<ulong, ApplicationInfo> Applications { get; internal init; } = new();

        public AppManager(List<IAppSource> sources)
        {
            Sources = sources.Select(x =>
            {
                Console.WriteLine($"Opening {x.Name}...");
                return x.Open();
            }).ToList();
            Layered = new LayeredFileSystem(Sources);

            Console.WriteLine("Importing tickets...");
            TikUtil.TryImport(Layered);

            Console.WriteLine("Parsing...");
            Merged = SwitchFs.OpenNcaDirectory(Keyset.Instance, Layered);

            Console.WriteLine("Indexing...");
            foreach (var (id, app) in Merged.Applications)
            {
                Applications[id] = new ApplicationInfo(app);
            }
            foreach (var (_, title) in Merged.Titles)
            {
                var app = Applications[title.Metadata.ApplicationTitleId];
                var version = title.Version.Version;

                Console.WriteLine($"Found {title.Metadata.Type} v{version} for {title.Name}");
                if (title.Metadata.Type == ContentMetaType.Patch)
                {
                    app.AddUpdate(title);
                }
            }
        }

        public void Dispose()
        {
            Sources.ForEach(x => x.Dispose());
        }
    }
}
