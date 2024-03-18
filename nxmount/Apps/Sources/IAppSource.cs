using LibHac.Fs.Fsa;

namespace nxmount.Apps.Sources
{
    public interface IAppSource
    {
        string Name { get; }

        IFileSystem Open();
    }
}
