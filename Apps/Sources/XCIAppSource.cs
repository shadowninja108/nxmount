using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibHac.Common;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using nxmount.Util;

namespace nxmount.Apps.AppSource
{
    public class XCIAppSource : FileAppSource
    {
        private Xci Xci;
        private XciPartition SecurePartition;

        public XCIAppSource(FileInfo file) : base(file)
        {
            Console.WriteLine($"Opening XCI {File.Name}...");
            Xci = new Xci(Keyset.Instance, Storage.Get);
            SecurePartition = Xci.OpenPartition(XciPartitionType.Secure);
        }

        public override IFileSystem Open()
        {
            return SecurePartition;
        }
    }
}
