using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.CustomisationExporter.Exporter;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

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

            //xlsx

            //first script generation of C# entities and fields
            var request = new CustomisationExporterRequest();
            request.IncludeAllRecordTypes = true;
            request.DuplicateManyToManyRelationshipSides = true;
            request.Entities = true;
            request.Fields = true;
            request.FieldOptionSets = true;
            request.Relationships = true;
            request.SharedOptionSets = true;
            request.IncludeOneToManyRelationships = true;
            request.SaveToFolder = new Folder(TestingFolder);

            var response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request.IncludeAllRecordTypes = true;
            request.DuplicateManyToManyRelationshipSides = false;
            request.Entities = true;
            request.Fields = false;
            request.FieldOptionSets = false;
            request.Relationships = true;
            request.SharedOptionSets = false;
            request.IncludeOneToManyRelationships = false;

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);
            
            response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request.IncludeAllRecordTypes = false;
            request.DuplicateManyToManyRelationshipSides = true;
            request.Entities = true;
            request.Fields = true;
            request.FieldOptionSets = true;
            request.Relationships = true;
            request.SharedOptionSets = true;
            request.IncludeOneToManyRelationships = true;
            request.RecordTypes = new[]
            {
                new RecordTypeSetting(Entities.account, Entities.account),
                new RecordTypeSetting(Entities.contact, Entities.contact)
            };

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);

            response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());

            request = new CustomisationExporterRequest
            {
                Format = CustomisationExporterRequest.FileFormat.Csv,
                IncludeAllRecordTypes = true,
                DuplicateManyToManyRelationshipSides = true,
                Entities = true,
                Fields = true,
                FieldOptionSets = true,
                Relationships = true,
                SharedOptionSets = true,
                IncludeOneToManyRelationships = true,
                SaveToFolder = new Folder(TestingFolder)
            };

            Thread.Sleep(1000);
            FileUtility.DeleteFiles(TestingFolder);

            response = testApplication.NavigateAndProcessDialog<CustomisationExporterModule, CustomisationExporterDialog, CustomisationExporterResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsTrue(FileUtility.GetFiles(TestingFolder).Any());
        }
    }
}