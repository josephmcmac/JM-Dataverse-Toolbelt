using $safeprojectname$.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace $safeprojectname$.Xrm
{
    public class VsixActiveXrmConnection : XrmConfiguration
    {
        public VsixActiveXrmConnection()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().CodeBase;
            var fileInfo = new FileInfo(assemblyLocation.Substring(8));
            var carryDirectory = fileInfo.Directory;
            while (carryDirectory.Parent != null)
            {
                if (carryDirectory
                    .Parent
                    .GetDirectories()
                    .Any(d => d.Name == "Xrm.Vsix"))
                {
                    carryDirectory = carryDirectory.Parent;
                    break;
                }
                carryDirectory = carryDirectory.Parent;
            }
            if (carryDirectory.Parent == null)
                throw new Exception("Error resolving connection file");
            var fileName = Path.Combine(carryDirectory.FullName, "Xrm.Vsix", Environment.UserName + ".solution.xrmconnection");
            var readEncryptedConfig = File.ReadAllText(fileName);
            var dictionary =
                    (Dictionary<string, string>)
                        JsonHelper.JsonStringToObject(readEncryptedConfig, typeof(Dictionary<string, string>));

            foreach (var prop in GetType().GetReadWriteProperties())
            {
                var value = dictionary[prop.Name];
                if (value != null && prop.Name == nameof(IXrmConfiguration.ClientSecret))
                    this.SetPropertyByString(prop.Name, new Password(value).GetRawPassword());
                else
                    this.SetPropertyByString(prop.Name, value);
            }
        }
    }
}