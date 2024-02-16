using DokanNet.Logging;
using DokanNet;
using nxmount.Apps;
using nxmount.Apps.AppSource;
using nxmount.Driver;
using nxmount.Util;

namespace nxmount
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var baseFi = new DirectoryInfo(@"Z:\Switch\Games\Bases");

            var sources = new List<IAppSource>();
            foreach (var fi in baseFi.EnumerateFiles())
            {
                switch (fi.Extension)
                {
                    case ".xci":
                        sources.Add(new XCIAppSource(fi));
                        break;
                    case ".nsp":
                        sources.Add(new NSPAppSource(fi));
                        break;
                }
            }

            var updateFi = new DirectoryInfo(@"Z:\Switch\Games\Splatoon 3");
            foreach (var fi in updateFi.EnumerateFiles("*.nsp", SearchOption.TopDirectoryOnly))
            {
                sources.Add(new NSPAppSource(fi));
            }

            var ncaFolders = new DirectoryInfo(@"Z:\Switch\Games\Splatoon 2\import");
            foreach (var fi in ncaFolders.EnumerateDirectories())
            {
                sources.Add(new NcaFolderAppSource(fi));
            }

            var mgr = new AppManager(sources);

            try
            {
                using (var mre = new ManualResetEvent(false))
                using (var dokanLogger = new ConsoleLogger("[Dokan] "))
                using (var dokan = new Dokan(dokanLogger))
                {
                    Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                    {
                        e.Cancel = true;
                        mre.Set();
                    };

                    var driver = new DokanDriver(mgr, ApplicationLanguage.AmericanEnglish);
                    var dokanBuilder = new DokanInstanceBuilder(dokan)
                        .ConfigureOptions(options =>
                        {
                            options.Options = /* DokanOptions.DebugMode | */ DokanOptions.StderrOutput;
                            options.MountPoint = "r:\\";
                        });
                    using (var dokanInstance = dokanBuilder.Build(driver))
                    {
                        mre.WaitOne();
                    }
                    Console.WriteLine(@"Success");
                }
            }
            catch (DokanException ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
        }
    }
}
