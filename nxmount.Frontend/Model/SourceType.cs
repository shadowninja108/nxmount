using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Frontend.Model
{
    public enum SourceType
    {
        [Description("NCA Folder")]
        NcaFolder,
        [Description("NSP")]
        Nsp,
        [Description("XCI")]
        Xci,
        [Description("NSP or XCI")]
        NspOrXci,
        [Description("SD Card")]
        Sd,
    }
}
