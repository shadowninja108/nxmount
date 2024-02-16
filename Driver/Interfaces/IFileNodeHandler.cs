using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet;

namespace nxmount.Driver.Interfaces
{
    public interface IFileNodeHandler : IFsNodeHandler
    {
        long Size { get; }

        bool Read(byte[] buffer, out int bytesRead, long offset);
    }
}
