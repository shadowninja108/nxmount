using LibHac.Common.Keys;

namespace nxmount.Util
{
    public class Keyset
    {
        /* Paths for loading keysets. */
        private static readonly DirectoryInfo UserProfileDirectoryInfo = new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        private static readonly DirectoryInfo SwitchDirectoryInfo = UserProfileDirectoryInfo.GetDirectory(".switch");
        private static readonly FileInfo ProdKeysPath = SwitchDirectoryInfo.GetFile("prod.keys");
        private static readonly FileInfo TitleKeysPath = SwitchDirectoryInfo.GetFile("title.keys");

        public static readonly KeySet Instance = new();

        static Keyset()
        {
            /* Check keys are actually present. */
            if (!ProdKeysPath.Exists)
            {
                Console.WriteLine($"Expects keyset at {ProdKeysPath.FullName}");
                Environment.Exit(0);
            }

            if (!TitleKeysPath.Exists)
            {
                Console.WriteLine($"Expects keyset at {TitleKeysPath.FullName}");
                Environment.Exit(0);
            }

            /* Load our keys. */
            ExternalKeyReader.ReadKeyFile(Instance, ProdKeysPath.FullName, TitleKeysPath.FullName, null, null);
        }
    }
}
