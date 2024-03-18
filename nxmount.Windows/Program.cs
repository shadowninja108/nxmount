using DokanNet;
using DokanNet.Logging;
using nxmount.Apps;
using nxmount.Apps.Sources;
using nxmount.Util;
using nxmount.Windows.Driver;

namespace nxmount.Windows
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sources = new List<IAppSource>();
            sources.Add(new SdCardAppSource(new DirectoryInfo(@"D:\")));
            //sources.AddFolderOfXcis(new DirectoryInfo(@"Z:\Switch\Games\Bases"));

            sources.AddFolderOfNspsOrXcis(new DirectoryInfo(@"Z:\Switch\Games\Bases"));
            //sources.AddFolderOfNsps(new DirectoryInfo(@"Z:\Switch\Games\Splatoon 3"));
            //sources.AddFolderOfNsps(new DirectoryInfo(@"Z:\Switch\Games\The Legend of Zelda Tears of the Kingdom"));
            // sources.AddFolderOfNsps(new DirectoryInfo(@"Z:\Switch\Games\Animal Crossing"));
            // sources.AddFolderOfNsps(new DirectoryInfo(@"Z:\Switch\Games\ARMS"));
            //sources.AddFolderOfNcaFolders(new DirectoryInfo(@"Z:\Switch\Games\Splatoon 2\import"));

            var mgr = new AppManager(sources);

            //MountWithDokan(mgr, ApplicationLanguage.AmericanEnglish);
            MountWithWinFsp(mgr, ApplicationLanguage.AmericanEnglish);
        }

        private static void MountWithDokan(AppManager mgr, ApplicationLanguage desiredLanguage)
        {
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

                    var driver = new DokanDriver(mgr,  desiredLanguage);
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

        private static void MountWithWinFsp(AppManager mgr, ApplicationLanguage desiredLanguage)
        {
            new WinFspService(mgr, desiredLanguage).Run();
        }
    }
}
