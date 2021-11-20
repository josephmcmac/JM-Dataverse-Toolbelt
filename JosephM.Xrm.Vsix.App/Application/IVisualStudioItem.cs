namespace JosephM.Xrm.Vsix.Application
{
    public interface IVisualStudioItem
    {
        string Name { get; }
        string NameOfContainingProject { get; }
        string FileName { get; }
    }
}