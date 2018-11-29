using JosephM.Record.Extentions;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.AddPortalCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixAddPortalCodeTests : JosephMVsixTests
    {
        /// <summary>
        /// Runs through the create package, deploy package, import solution and import records dialogs
        /// </summary>

        [TestMethod]
        public void VsixAddPortalCodeTest()
        {
            RecreatePortalData(createSecondDuplicateSite: true);
            var app = CreateAndLoadTestApplication<AddPortalCodeModule>();

            var fakeProjectName = "FakeProjectName";
            var directoryInfo = Directory.CreateDirectory(Path.Combine(VisualStudioService.SolutionDirectory, fakeProjectName));
            VisualStudioService.SetSelectedItem(new FakeVisualStudioSolutionFolder(directoryInfo.FullName));

            var websiteName = "Fake Site 1";
            var webSite = XrmRecordService.GetFirst(Entities.adx_website, Fields.adx_website_.adx_name, websiteName);
            var request = new AddPortalCodeRequest
            {
                ProjectName = fakeProjectName,
                WebSite = XrmRecordService.ToLookup(webSite),
                ExportWhereFieldEmpty = true,
                CreateFolderForWebsiteName = true
            };

            var responseViewModel = app.NavigateAndProcessDialogGetResponseViewModel<AddPortalCodeModule, AddPortalCodeDialog>(request);
            var response = responseViewModel.GetObject() as AddPortalCodeResponse;
            Assert.IsFalse(response.HasError);

            var rootFolder = Path.Combine(directoryInfo.FullName, websiteName);
            foreach (var typesFolder in new DirectoryInfo(rootFolder).GetDirectories())
            {
                var fileCountInDirectory = typesFolder.GetFiles().Count();
                var typeLabel = typesFolder.Name;
                var type = XrmRecordService.GetAllRecordTypes().First(rt => XrmRecordService.GetDisplayName(rt) == typeLabel);
                var recordsOfType = XrmRecordService.RetrieveAll(type, null);
                if(type == Entities.adx_webpage)
                {
                    //web pages have a parent and child where onluy the ch9ild is exported
                    // (multi language not implemented)
                    // + each has html, css & javascript
                    Assert.AreEqual((recordsOfType.Count() / 4) * 3, fileCountInDirectory);
                }
                else
                {
                    Assert.AreEqual(recordsOfType.Count() / 2, fileCountInDirectory);
                }
            }

        }
    }
}
