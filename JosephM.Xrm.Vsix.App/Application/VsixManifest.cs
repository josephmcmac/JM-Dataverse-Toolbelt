using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace JosephM.Xrm.Vsix.Application
{
    public class VsixManifest
    {
        /*
<?xml version="1.0" encoding="utf-8"?>
<PackageManifest Version="2.0.0" xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011" xmlns:d="http://schemas.microsoft.com/developer/vsx-schema-design/2011">
 <Metadata>
   <Identity Id="Company.Product..7c83a90b-fbd2-4b28-b79b-40b90ff7fa97" Version="0.1.0.0" Language="en-US" Publisher="Company" />
   <DisplayName>Company Product</DisplayName>
   <Description>Lorem ipsum dolor sit amet, consectetur adipiscing elit. ALIO MODO. Quonam modo? Tu vero, inquam, ducas licet, si sequetur</Description>
   <Icon>Resources\Package.ico</Icon>
 </Metadata>
 <Installation InstalledByMsi="false">
   <InstallationTarget Id="Microsoft.VisualStudio.Pro" Version="[11.0,13.0)" />
 </Installation>
 <Dependencies>
   <Dependency Id="Microsoft.Framework.NDP" DisplayName="Microsoft .NET Framework" d:Source="Manual" Version="4.5" />
   <Dependency Id="Microsoft.VisualStudio.MPF.11.0" DisplayName="Visual Studio MPF 11.0" d:Source="Installed" Version="11.0" />
 </Dependencies>
 <Assets>
   <Asset Type="Microsoft.VisualStudio.VsPackage" d:Source="Project" d:ProjectName="%CurrentProject%" Path="|%CurrentProject%;PkgdefProjectOutputGroup|" />
 </Assets>
</PackageManifest>
         */

        public string Id { get; set; }

        public string Version { get; set; }

        public VsixManifest()
        {

        }

        public VsixManifest(string manifestPath)
        {
            var doc = new XmlDocument();
            doc.Load(manifestPath);

            if (doc.DocumentElement == null || doc.DocumentElement.Name != "PackageManifest") return;

            var metaData = doc.DocumentElement.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Metadata");
            var identity = metaData.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Identity");

            Id = identity.GetAttribute("Id");
            Version = identity.GetAttribute("Version");
        }

        public static VsixManifest GetManifest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyUri = new UriBuilder(assembly.CodeBase);
            var assemblyPath = Uri.UnescapeDataString(assemblyUri.Path);
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            var vsixManifestPath = Path.Combine(assemblyDirectory, "extension.vsixmanifest");

            return new VsixManifest(vsixManifestPath);
        }
    }
}