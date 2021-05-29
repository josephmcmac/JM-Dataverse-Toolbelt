using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Core.FieldType;
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
                WebSite = XrmRecordService.ToLookup(webSite),
                ExportWhereFieldEmpty = true,
                CreateFolderForWebsiteName = true
            };

            var dialog = app.NavigateToDialog<AddPortalCodeModule, AddPortalCodeDialog>();
            var entryForm = app.GetSubObjectEntryViewModel(dialog);
            entryForm.GetLookupFieldFieldViewModel(nameof(AddPortalCodeRequest.WebSite)).SetValue(entryForm.GetLookupFieldFieldViewModel(nameof(AddPortalCodeRequest.WebSite)).ItemsSourceAsync.First(r => r.Record != null).Record);
            entryForm.GetBooleanFieldFieldViewModel(nameof(AddPortalCodeRequest.ExportWhereFieldEmpty)).Value = true;
            entryForm.GetBooleanFieldFieldViewModel(nameof(AddPortalCodeRequest.CreateFolderForWebsiteName)).Value = true;

            var section = entryForm.GetFieldSection(AddPortalCodeRequest.Sections.RecordsToInclude);
            var func = section.CustomFunctions.First(c => c.Id == "SELECTALL");
            func.Invoke();
            Assert.IsTrue(entryForm.GetEnumerableFieldViewModel(nameof(AddPortalCodeRequest.RecordsToExport)).GridRecords
                .All(r => r.GetBooleanFieldFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.Selected)).Value ?? false));

            var webTemplateRow = entryForm.GetEnumerableFieldViewModel(nameof(AddPortalCodeRequest.RecordsToExport))
                .GridRecords
                .First(gr => gr.GetRecordTypeFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.RecordType)).Value.Key == Entities.adx_webtemplate);

            webTemplateRow.GetBooleanFieldFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.IncludeAll)).Value = false;
            //this now auto runs when the flag above iunchecked
            //webTemplateRow.GetEnumerableFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.RecordsToInclude)).BulkAddButton.Invoke();

            var templateRecordSelectionForm = entryForm.ChildForms.First() as MultiSelectDialogViewModel<PicklistOption>;
            templateRecordSelectionForm.ItemsSource.First().Select = true;
            templateRecordSelectionForm.ItemsSource.Last().Select = true;
            templateRecordSelectionForm.ApplyButtonViewModel.Invoke();
            Assert.IsFalse(entryForm.ChildForms.Any());

            Assert.IsTrue(entryForm.Validate());
            entryForm.SaveButtonViewModel.Invoke();


            var responseViewModel = app.GetCompletionViewModel(dialog);
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
                else if (type == Entities.adx_webtemplate)
                {
                    Assert.AreEqual(2, fileCountInDirectory);
                }
                else
                {
                    Assert.AreEqual(recordsOfType.Count() / 2, fileCountInDirectory);
                }
            }
        }
    }
}
