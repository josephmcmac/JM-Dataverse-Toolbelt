#region

using System.Collections.Generic;
using System.IO;

#endregion

namespace JosephM.Core.Utility
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

        public static void DeleteSubFolders(string folder)
        {
            if (Directory.Exists(folder))
                foreach (var subFolder in Directory.GetDirectories(folder))
                {
                    Directory.Delete(subFolder);
                }
        }

        public static IEnumerable<string> GetFiles(string folder)
        {
            return Directory.GetFiles(folder);
        }

        public static void WriteToFile(string folder, string name, string text)
        {
            CheckCreateFolder(folder);
            File.WriteAllText(folder + @"\" + name, text);
        }

        public static void WriteToFile(string folder, string name, byte[] bytes)
        {
            CheckCreateFolder(folder);
            File.WriteAllBytes(folder + @"\" + name, bytes);
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