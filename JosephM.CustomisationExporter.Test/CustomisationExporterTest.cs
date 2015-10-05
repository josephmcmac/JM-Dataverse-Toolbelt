using System.Linq;
using System.Threading;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.CustomisationExporter.Exporter;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.CustomisationExporter.Test
{
    [TestClass]
    public class CustomisationExporterTest : XrmModuleTest
    {
        [TestMethod]
        public void CustomisationExporterTestExport()
        {
            FileUtility.DeleteFiles(TestingFolder);
            Assert.IsFalse(FileUtility.GetFiles(TestingFolder).Any());

            //okay script through generation of the three types

            //create test application with module loaded
            var testApplication = CreateAndLoadTestApplication<CustomisationExporterModule>();

            //first script generation of C# entities and fields
            var request = new CustomisationExporterRequest();
            request.AllRecordTypes = true;
            request.DuplicateManyToManyRelationshipSides = true;
            request.ExportEntities = true;
            request.ExportFields = true;
            request.ExportOptionSets = true;
            request.ExportRelationships = true;
            request.ExportSharedOptionSets = true;
            request.IncludeOneToManyRelationships = true;
            request.SaveToFolder = new Folder(TestingFolder);

            testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog>(request);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request.AllRecordTypes = true;
            request.DuplicateManyToManyRelationshipSides = false;
            request.ExportEntities = true;
            request.ExportFields = false;
            request.ExportOptionSets = false;
            request.ExportRelationships = true;
            request.ExportSharedOptionSets = false;
            request.IncludeOneToManyRelationships = false;

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);
            
            testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog>(request);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request.AllRecordTypes = false;
            request.DuplicateManyToManyRelationshipSides = true;
            request.ExportEntities = true;
            request.ExportFields = true;
            request.ExportOptionSets = true;
            request.ExportRelationships = true;
            request.ExportSharedOptionSets = true;
            request.IncludeOneToManyRelationships = true;
            request.RecordTypes = new[]
            {
                new RecordTypeSetting(Entities.account, Entities.account),
                new RecordTypeSetting(Entities.contact, Entities.contact)
            };

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);
        }
    }
}