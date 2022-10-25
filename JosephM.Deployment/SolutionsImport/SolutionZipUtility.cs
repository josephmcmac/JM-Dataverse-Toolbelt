using System;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace JosephM.Deployment.SolutionsImport
{
    public static class SolutionZipUtility
    {
        public static SolutionZipMetadata LoadSolutionZipMetadata(string file)
        {
            using (var archive = ZipFile.OpenRead(file))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName == "solution.xml")
                    {
                        using (var stream = entry.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                var solutionXmlContent = reader.ReadToEnd();
                                var solutionXmlDocument = new XmlDocument();
                                solutionXmlDocument.LoadXml(solutionXmlContent);
                                return new SolutionZipMetadata
                                {
                                    Managed = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/Managed")?.InnerText == "1",
                                    Version = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/Version")?.InnerText,
                                    UniqueName = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/UniqueName")?.InnerText,
                                    FriendlyName = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/LocalizedNames/LocalizedName/@description")?.InnerText
                                };
                            }
                        }
                    }
                }
            }
            throw new Exception($"Could not load solution details from file {file}. solution.xml was not found in the zip file");
        }

        public static SolutionZipMetadata LoadSolutionZipMetadata(byte[] bytes)
        {
            using (var byteStream = new MemoryStream(bytes))
            {
                using (var archive = new ZipArchive(byteStream))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName == "solution.xml")
                        {
                            using (var stream = entry.Open())
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    var solutionXmlContent = reader.ReadToEnd();
                                    var solutionXmlDocument = new XmlDocument();
                                    solutionXmlDocument.LoadXml(solutionXmlContent);
                                    return new SolutionZipMetadata
                                    {
                                        Managed = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/Managed")?.InnerText == "1",
                                        Version = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/Version")?.InnerText,
                                        UniqueName = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/UniqueName")?.InnerText,
                                        FriendlyName = solutionXmlDocument.SelectSingleNode("/ImportExportXml/SolutionManifest/LocalizedNames/LocalizedName/@description")?.InnerText
                                    };
                                }
                            }
                        }
                    }
                }
            }
            throw new Exception($"Could not load solution details from byte array. solution.xml was not found in the zip file");
        }
    }
}
