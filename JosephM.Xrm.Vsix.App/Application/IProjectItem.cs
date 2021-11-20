namespace JosephM.Xrm.Vsix.Application
{
    public interface IProjectItem : IVisualStudioItem
    {
        void SetProperty(string propertyName, object value);
        string FileFolder { get; }
    }
}