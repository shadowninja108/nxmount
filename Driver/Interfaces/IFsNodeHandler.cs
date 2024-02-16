using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Driver.Interfaces
{
    public interface IFsNodeHandler
    {
        public IFsNodeHandler? Parent { get;  }
        public string FileName { get; }
        bool QueryFileInfo(Context context, ref FileInformation info);
    }
}
