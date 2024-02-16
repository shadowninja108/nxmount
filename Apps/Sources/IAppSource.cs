using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;

namespace nxmount.Apps.AppSource
{
    public interface IAppSource
    {
        string Name { get; }

        IFileSystem Open();
    }
}
