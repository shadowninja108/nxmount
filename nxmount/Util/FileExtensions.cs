namespace nxmount.Util
{
    public static class FileExtensions
    {
        public static DirectoryInfo GetDirectory(this DirectoryInfo dir, string sub)
        {
            return new DirectoryInfo(Path.Combine(dir.FullName, sub));
        }

        public static FileInfo GetFile(this DirectoryInfo dir, string name)
        {
            return new FileInfo(Path.Combine(dir.FullName, name));
        }
    }
}
