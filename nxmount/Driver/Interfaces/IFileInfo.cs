namespace nxmount.Driver.Interfaces
{
    public interface IFileInfo
    {
        public int FileNameHash { get; }
        public string FileName { get; set; }
        public FileAttributes Attributes { get; set; }
        public long Length { get; set; }
    }
}
