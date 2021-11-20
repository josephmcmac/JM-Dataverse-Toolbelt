namespace JosephM.Xrm.Vsix.Application
{
    public interface IVisualStudioProject
    {
        string Name { get; }
        IProjectItem AddProjectItem(string file);
        void AddItem(string fileName, string fileContent, params string[] folderPath);
    }
}