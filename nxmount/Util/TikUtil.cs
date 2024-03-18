using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Spl;
using LibHac.Tools.Es;
using LibHac.Tools.FsSystem;

namespace nxmount.Util
{
    public static class TikUtil
    {
        public static RightsId GetRightsId(this Ticket ticket)
        {
            return new RightsId(ticket.RightsId);
        }

        public static AccessKey? GetAccessKey(this Ticket ticket)
        {
            var tk = ticket.GetTitleKey(Keyset.Instance);
            if (tk == null)
                return null;

            return new AccessKey(tk);
        }

        public static bool TryImport(IFileSystem fs)
        {
            bool success = true;
            foreach (var entry in fs.EnumerateEntries("*.tik", SearchOptions.Default))
            {
                var reff = new UniqueRef<IFile>();
                fs.OpenFile(ref reff.Ref, entry.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();
                using var stream = new NxFileStream(reff.Get, OpenMode.Read, false);
                success &= TryImport(stream);
            }
            return success;
        }

        public static bool TryImport(Stream stream)
        {
            try
            {
                var t = new Ticket(stream);
                var eks = Keyset.Instance.ExternalKeySet;
                var rightsId = t.GetRightsId();

                var tk = t.GetAccessKey();
                if (!tk.HasValue)
                {
                    Console.WriteLine($"Error: Failed to decrypt title key for rights id {rightsId.DebugDisplay()}!");
                    return false;
                }

                if (eks.Contains(rightsId))
                {
                    Console.WriteLine($"Warning: {rightsId.DebugDisplay()} already exists in keyset, overwriting.");
                    eks.Remove(rightsId);
                }

                eks.Add(rightsId, tk.Value);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
