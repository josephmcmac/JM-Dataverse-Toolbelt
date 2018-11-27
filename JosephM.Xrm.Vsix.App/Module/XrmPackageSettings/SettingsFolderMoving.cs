using JosephM.Core.Attributes;

namespace JosephM.Xrm.Vsix.Module.PackageSettings
{
    [Group(Sections.CodeChanges, false, order: 30)]
    [Instruction("This window provides details of changes which have been made to the storing of setting in this Visual Studio extention. The changes which have been made are"
                    + "\n"
                    + "\n1. The folder used has been changed from SolutionItems to Xrm.Vsix"
                    + "\n2. The package settings and connections are now stored in personalised settings files"
                    + "\n"
                    + "\nIf you use an existing instance of the solution template test project, this means you will have to update code to load the correct personal setting file"
                    + "\n"
                    + "\nThe specific code which requires changing is the propery named XrmConfiguration in the XrmTest class as shown in the details below"
                    + "\n"
                    + "\nOnce the new personalised settings files are created, The old files may be safely removed from the solution and deleted on disk")]
    public class SettingsFolderMoving
    {
        [DisplayOrder(110)]
        [Group(Sections.CodeChanges)]
        public string ReplaceThisLineOfCode
        {
            get
            {
                return "var readEncryptedConfig = \"File.ReadAllText(\"solution.xrmconnection\")";
            }
        }

        [DisplayOrder(120)]
        [Group(Sections.CodeChanges)]
        public string WithTheseLines
        {
            get
            {
                return "var assemblyLocation = Assembly.GetExecutingAssembly().CodeBase;"
                +"\nvar fileInfo = new FileInfo(assemblyLocation.Substring(8));"
                + "\nvar carryDirectory = fileInfo.Directory;"
                + "\nvar nameOfRootFolderInSolution = new[] { \"TestResults\", Assembly.GetExecutingAssembly().GetName().Name };"
                + "\nwhile (carryDirectory.Parent != null && !nameOfRootFolderInSolution.Contains(carryDirectory.Name))"
                + "\n{"
                + "\n    carryDirectory = carryDirectory.Parent;"
                + "\n    if (nameOfRootFolderInSolution.Contains(carryDirectory.Name))"
                + "\n    {"
                + "\n        carryDirectory = carryDirectory.Parent;"
                + "\n        break;"
                + "\n    }"
                + "\n}"
                + "\nif (carryDirectory.Parent == null)"
                + "\n    throw new Exception(\"Error resolving connection file\");"
                + "\nvar fileName = Path.Combine(carryDirectory.FullName, \"Xrm.Vsix\", Environment.UserName + \".solution.xrmconnection\");"
                + "\nvar readEncryptedConfig = File.ReadAllText(fileName); ";
            }
        }

        private static class Sections
        {
            public const string CodeChanges = "Code Changes";
        }
    }
}
