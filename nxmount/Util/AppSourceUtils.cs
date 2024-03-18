using nxmount.Apps.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Util
{
    public static class AppSourceUtils
    {
        public static void AddNsp(this List<IAppSource> sources, FileInfo fi) => sources.Add(new NSPAppSource(fi));
        public static void AddXci(this List<IAppSource> sources, FileInfo fi) => sources.Add(new XCIAppSource(fi));

        public static void AddNcaFolder(this List<IAppSource> sources, DirectoryInfo di) => sources.Add(new NcaFolderAppSource(di));

        public static void AddFolderOfNsps(this List<IAppSource> sources, DirectoryInfo di)
        {
            foreach (var fi in di.EnumerateFiles("*.nsp", SearchOption.TopDirectoryOnly))
            {
                sources.AddNsp(fi);
            }
        }

        public static void AddFolderOfXcis(this List<IAppSource> sources, DirectoryInfo di)
        {
            foreach (var fi in di.EnumerateFiles("*.xci", SearchOption.TopDirectoryOnly))
            {
                sources.AddXci(fi);
            }
        }

        public static void AddFolderOfNspsOrXcis(this List<IAppSource> sources, DirectoryInfo di)
        {
            foreach (var fi in di.EnumerateFiles())
            {
                switch (fi.Extension)
                {
                    case ".xci":
                        sources.AddXci(fi);
                        break;
                    case ".nsp":
                        sources.AddNsp(fi);
                        break;
                }
            }
        }

        public static void AddFolderOfNcaFolders(this List<IAppSource> sources, DirectoryInfo di)
        {
            foreach (var child in di.EnumerateDirectories())
            {
                sources.AddNcaFolder(child);
            }
        }
    }
}
