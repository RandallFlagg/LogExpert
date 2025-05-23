namespace LogExpert.Core.Interface
{
    public interface IPluginRegistry
    {
        IList<ILogLineColumnizer> RegisteredColumnizers { get; }

        IFileSystemPlugin FindFileSystemForUri(string fileNameOrUri);
    }
}