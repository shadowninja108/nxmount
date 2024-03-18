using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using nxmount.Util;

namespace nxmount.Apps.Sources
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
