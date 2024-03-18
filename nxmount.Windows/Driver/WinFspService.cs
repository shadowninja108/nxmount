using Fsp;
using nxmount.Apps;
using nxmount.Util;

namespace nxmount.Windows.Driver
{
    public class WinFspService : Service, IMountService
    {
        private AppManager AppManager;
        private ApplicationLanguage DesiredLanguage;
        private EventWaitHandle WaitStartupEvent = new(false, EventResetMode.ManualReset);

        private FileSystemHost? Host;
        public string MountPoint => Host?.MountPoint() ?? string.Empty;

        public WinFspService(AppManager appManager, ApplicationLanguage desiredLanguage) : base("nxmount")
        {
            AppManager = appManager;
            DesiredLanguage = desiredLanguage;
        }

        protected override void OnStart(string[] Args)
        {
            Host = new FileSystemHost(new WinFspDriver(AppManager, DesiredLanguage));
            Host!.UnicodeOnDisk = true;
            Host!.FileInfoTimeout = unchecked((uint)-1);
            Host!.FileSystemName = "nxmount";
            Host!.MountEx("R:", 4, DebugLog: unchecked((uint)-1));

            WaitStartupEvent.Set();
        }

        protected override void OnStop()
        {
            Host!.Unmount();
            Host = null;
        }

        public event IMountService.LogEventHandler? MessageHandler;


        public void Start()
        {
            new Thread(() =>
            {
                MessageHandler?.Invoke("Starting service...");
                var result = Run();
                ;
            }).Start();
            WaitStartupEvent.WaitOne();
        }

        public void End()
        {
            MessageHandler?.Invoke("Stopping service...");
            Stop();
        }
    }
}
