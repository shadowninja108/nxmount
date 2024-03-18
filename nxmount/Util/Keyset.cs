using LibHac.Common.Keys;

namespace nxmount.Util
{
    public class Keyset
    {
        private static readonly string SwitchFolderName = ".switch";
        private static readonly string ProdKeysName = "prod.keys";
        private static readonly string TitleKeysName = "title.keys";

        /* Paths for loading keysets. */
        private static readonly DirectoryInfo WorkingDirectory = new(Environment.CurrentDirectory);
        private static readonly DirectoryInfo UserProfileDirectoryInfo = new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        private static readonly DirectoryInfo SwitchDirectoryPathInUserProfile = UserProfileDirectoryInfo.GetDirectory(SwitchFolderName);
        private static readonly FileInfo ProdKeysPathInUserProfile = SwitchDirectoryPathInUserProfile.GetFile(ProdKeysName);
        private static readonly FileInfo TitleKeysPathInUserProfile = SwitchDirectoryPathInUserProfile.GetFile(TitleKeysName);

        private static readonly DirectoryInfo SwitchDirectoryPathInWorkingDirectory = WorkingDirectory.GetDirectory(SwitchFolderName);
        private static readonly FileInfo ProdKeysPathInWorkingDirectory = SwitchDirectoryPathInWorkingDirectory.GetFile(ProdKeysName);
        private static readonly FileInfo TitleKeysPathInWorkingDirectory = SwitchDirectoryPathInWorkingDirectory.GetFile(TitleKeysName);

        public static readonly KeySet Instance = new();

        static Keyset()
        {
            /* Check keys are actually present. */

            /* Prefer keys in working directory first, then fallback to user profile. */
            var prodFi = ProdKeysPathInWorkingDirectory;
            if (!prodFi.Exists)
                prodFi = ProdKeysPathInUserProfile;

            if (!prodFi.Exists)
            {
                Console.WriteLine($"Expects keyset at {ProdKeysPathInWorkingDirectory.FullName} {ProdKeysPathInUserProfile.FullName}");
                Environment.Exit(0);
            }

            var titleFi = TitleKeysPathInWorkingDirectory;
            if (!titleFi.Exists)
                titleFi = TitleKeysPathInUserProfile;

            if (!titleFi.Exists)
            {
                Console.WriteLine($"Expects keyset at {TitleKeysPathInWorkingDirectory.FullName} {TitleKeysPathInUserProfile.FullName}");
                Environment.Exit(0);
            }

            /* Load our keys. */
            ExternalKeyReader.ReadKeyFile(Instance, prodFi.FullName, titleFi.FullName, null, null);
        }
    }
}
