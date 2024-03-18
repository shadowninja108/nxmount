using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuseDotNet;
using nxmount.Apps;
using nxmount.Util;

namespace nxmount.Linux.Driver
{
    public class NxFuseService : FuseService, IMountService
    {
        public NxFuseService(AppManager manager, ApplicationLanguage desiredLanguage, string mountPoint) : base(new FuseDriver(manager, desiredLanguage), [mountPoint, mountPoint])
        {
            Error += (sender, e) =>
            {
                Console.WriteLine("Error");
                Console.WriteLine(sender);
                Console.WriteLine(e);
            };
        }

        public event IMountService.LogEventHandler? MessageHandler;

        protected override void OnError(ThreadExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
        }

        public void Start()
        {
            new Thread(base.Start).Start();
        }

        public void End()
        {
            WaitExit();
        }
    }
}
