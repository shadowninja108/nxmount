using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount
{
    public interface IMountService
    {
        delegate void LogEventHandler(string message);

        event LogEventHandler MessageHandler;

        string MountPoint { get; }
        void Start();
        void End();
    }
}
