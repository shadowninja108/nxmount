using DokanNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Driver.Interfaces
{
    public interface IDirectoryHandler : IFsNodeHandler
    {
        bool QueryChildren(Context context, ReadOnlySpan<string> pathComponents, ref IList<FileInformation> files);
        bool QueryNode(Context context, ReadOnlySpan<string> pathComponents, out IFsNodeHandler? info);
    }
}
