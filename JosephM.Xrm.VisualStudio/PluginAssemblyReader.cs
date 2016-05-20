using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.XRM.VSIX
{
    public class PluginAssemblyReader : MarshalByRefObject
    {
        public IEnumerable<PluginType> LoadTypes(string assemblyFile)
        {
            var results = new List<PluginType>();

            var loaded = new List<Assembly>();

            var fileInfo = new FileInfo(assemblyFile);

            //load the assemblies in same folder as assembly
            var dir = fileInfo.DirectoryName;
            var files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                if (file.EndsWith(".dll") && new FileInfo(file).Name != fileInfo.Name)
                {
                    var loadAssembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(file));
                    loaded.Add(loadAssembly);
                }
            }
            var name = new AssemblyName();
            name.CodeBase = assemblyFile;
            var assembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(assemblyFile));
            var references = assembly.GetReferencedAssemblies();
            Type codeActivityType = null;
            //load assembly references not in the bin folder
            foreach (var reference in references)
            {
                if (loaded.All(a => a.FullName != reference.FullName))
                {
                    var loadAssembly = Assembly.ReflectionOnlyLoad(reference.FullName);
                    loaded.Add(loadAssembly);
                    //if there is a reference to System.Activities (the assembly for workflow activities)
                    //then get the codeactivity type to check for workflow implementations
                    if (loadAssembly.GetName().Name == "System.Activities")
                    {
                        var references2 = loadAssembly.GetReferencedAssemblies();
                        foreach (var reference2 in references2)
                        {
                            if (loaded.All(a => a.FullName != reference2.FullName))
                            {
                                var loadAssembly2 = Assembly.ReflectionOnlyLoad(reference2.FullName);
                                loaded.Add(loadAssembly2);
                            }
                        }
                        codeActivityType = loadAssembly.GetExportedTypes().First(t => t.Name == "CodeActivity");
                    }
                }
            }

            foreach (var type in assembly.GetExportedTypes()
                                        .Where(t => !t.IsAbstract))
            {
                if (type.GetInterface("Microsoft.Xrm.Sdk.IPlugin", true) != null)
                {
                    results.Add(new PluginType(PluginType.XrmPluginType.Plugin, type.FullName));
                }
            }
            if (codeActivityType != null)
            {
                foreach (var type in assembly.GetExportedTypes()
                                        .Where(t => !t.IsAbstract))
                {
                    if (type.IsSubclassOf(codeActivityType))
                    {
                        results.Add(new PluginType(PluginType.XrmPluginType.WorkflowActivity, type.FullName));
                    }
                }
            }

            return results;
        }

        public class PluginType : MarshalByRefObject
        {
            public PluginType(XrmPluginType type, string typeName)
            {
                TypeName = typeName;
                Type = type;
            }

            public string TypeName { get; set; }

            public enum XrmPluginType
            {
                Plugin,
                WorkflowActivity
            }

            public XrmPluginType Type { get; set; }
        }
    }
}
