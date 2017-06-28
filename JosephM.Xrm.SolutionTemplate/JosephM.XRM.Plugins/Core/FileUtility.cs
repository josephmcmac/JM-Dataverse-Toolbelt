#region

using System.Collections.Generic;
using System.IO;

#endregion

namespace $safeprojectname$.Core
{
    public static class FileUtility
    {
        public static void DeleteFiles(string folder)
        {
            if (Directory.Exists(folder))
                foreach (var file in GetFiles(folder))
                {
                    File.Delete(file);
                }
        }

        public static IEnumerable<string> GetFiles(string folder)
        {
            return Directory.GetFiles(folder);
        }

        public static void WriteToFile(string folder, string name, string text)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                CheckCreateFolder(folder);
                File.WriteAllText(folder + @"\" + name, text);
            }
            else
                File.WriteAllText(name, text);
        }

        public static void AppendToFile(string folder, string name, string text)
        {
            CheckCreateFolder(folder);
            File.AppendAllText(folder + @"\" + name, text);
        }

        public static void CheckCreateFolder(string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        internal static void CheckCreateFile(string folder, string name, string text)
        {
            CheckCreateFolder(folder);
            WriteToFile(folder, name, text);
        }
    }
}