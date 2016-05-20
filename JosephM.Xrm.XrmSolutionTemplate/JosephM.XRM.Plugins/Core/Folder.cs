namespace $safeprojectname$.Core
{
    public class Folder
    {
        public Folder(string folderPath)
        {
            FolderPath = folderPath;
        }

        public string FolderPath { get; set; }

        public override string ToString()
        {
            return FolderPath;
        }
    }
}