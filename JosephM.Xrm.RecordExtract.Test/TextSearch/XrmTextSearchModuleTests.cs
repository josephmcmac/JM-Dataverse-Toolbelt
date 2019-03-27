using JosephM.Application.Desktop.Module.Crud.BulkReplace;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Xrm.RecordExtract.TextSearch;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.RecordExtract.Test.TextSearch
{
    [TestClass]
    public class XrmTextSearchModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void XrmTextSearchModuleTest()
        {
            PrepareTests(true);

            //okay lets script through a text search - and do a replace all for one of the results
            //lets only do it for account with various fields a field specified as html
            var searchString = "uhhuh";

            //creta seom sample accounts to check
            DeleteAll(Entities.account);
            var account1 = CreateAccount();
            account1.SetField(Fields.account_.name, $"Why {searchString} Not");
            account1.SetField(Fields.account_.description, $"<p><a href='{searchString}'>Nope</a></p>"); //this shouldnt match for stripped html search
            account1 = UpdateFieldsAndRetreive(account1, Fields.account_.name, Fields.account_.description);
            var account2 = CreateAccount();
            account2.SetField(Fields.account_.name, searchString);
            account2.SetField(Fields.account_.description, $"<p>{searchString}</p>");
            account2 = UpdateFieldsAndRetreive(account2, Fields.account_.name, Fields.account_.description);
            var account3 = CreateAccount();
            account3.SetField(Fields.account_.fax, "o" + searchString + "o");
            account3 = UpdateFieldsAndRetreive(account3, Fields.account_.name, Fields.account_.fax);
            var account4 = CreateAccount();

            //run a search request
            var application = CreateAndLoadTestApplication<XrmTextSearchModule>();
            var instance = new TextSearchRequest();
            instance.SearchTerms = new[]
            {
                new TextSearchRequest.SearchTerm() { Text = searchString }
            };
            instance.GenerateDocument = true;
            instance.DocumentFormat = DocumentWriter.DocumentType.Pdf;
            instance.SaveToFolder = new Folder(TestingFolder);
            instance.SearchAllTypes = false;
            instance.TypesToSearch = new[]
            {
                new TextSearchRequest.TypeToSearch { RecordType = new RecordType(Entities.account, Entities.account) }
            };
            instance.StripHtmlTagsPriorToSearch = true;
            instance.CustomHtmlFields = new[]
            {
                new RecordFieldSetting() { RecordType = new RecordType(Entities.account, Entities.account), RecordField = new RecordField(Fields.account_.description, Fields.account_.description) }
            };

            var responseViewModel = application.NavigateAndProcessDialogGetResponseViewModel<XrmTextSearchModule, XrmTextSearchDialog>(instance);
            var response = responseViewModel.GetObject() as TextSearchResponse;
            Assert.IsFalse(response.HasError);

            //verify the counts correct for the fields I populated
            Assert.AreEqual(3, response.Summary.First(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == "any").NumberOfMatches);
            Assert.AreEqual(2, response.Summary.First(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == Fields.account_.name).NumberOfMatches);
            Assert.AreEqual(1, response.Summary.First(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == Fields.account_.description).NumberOfMatches);
            Assert.AreEqual(1, response.Summary.First(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == Fields.account_.fax).NumberOfMatches);

            //okay we also have a feature where we load the results into a grid
            var summaryGrid = responseViewModel.GetEnumerableFieldViewModel(nameof(TextSearchResponse.Summary));
            Assert.AreEqual(4, summaryGrid.GridRecords.Count);
            summaryGrid.DynamicGridViewModel.GridRecords.First(gr => gr.GetStringFieldFieldViewModel(nameof(TextSearchResponse.SummaryItem.MatchedField)).Value == XrmService.GetFieldLabel(Fields.account_.name, Entities.account)).IsSelected = true;
            summaryGrid.DynamicGridViewModel.GetButton("LOADTOGRID").Invoke();

            var editResultsDialog = responseViewModel.ChildForms.First() as EditResultsDialog;
            Assert.IsNotNull(editResultsDialog);
            editResultsDialog.Controller.BeginDialog();
            var summaryResultsGrid = editResultsDialog.Controller.UiItems.First() as DynamicGridViewModel;
            Assert.IsNotNull(summaryResultsGrid);

            Assert.AreEqual(2, summaryResultsGrid.GridRecords.Count);

            //which has a replace option -lets try it
            summaryResultsGrid.GetButton("BULKREPLACEALL").Invoke();
            DoBulkReplace(editResultsDialog, Fields.account_.name, searchString, "nope");

            //return to completion/summary form
            summaryResultsGrid.GetButton("BACKTOSUMMARY").Invoke();
            Assert.IsFalse(responseViewModel.ChildForms.Any());

            //now we did the replace all lets just verify the name no longer matched in the text search reuslt
            var responseViewModel2 = application.NavigateAndProcessDialogGetResponseViewModel<XrmTextSearchModule, XrmTextSearchDialog>(instance);
            var response2 = responseViewModel2.GetObject() as TextSearchResponse;
            Assert.IsFalse(response2.HasError);

            Assert.AreEqual(2, response2.Summary.First(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == "any").NumberOfMatches);
            Assert.IsFalse(response2.Summary.Any(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == Fields.account_.name));
            Assert.AreEqual(1, response2.Summary.First(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == Fields.account_.description).NumberOfMatches);
            Assert.AreEqual(1, response2.Summary.First(s => s.RecordTypeSchemaName == Entities.account && s.MatchedFieldSchemaName == Fields.account_.fax).NumberOfMatches);

        }

        private static void DoBulkReplace(EditResultsDialog parentDialog, string field, string oldValue, string newValue)
        {
            var dialog = parentDialog.ChildForms.First() as BulkReplaceDialog;
            Assert.IsNotNull(dialog);
            dialog.LoadDialog();
            var entryViewModel = dialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(entryViewModel);
            entryViewModel.LoadFormSections();
            var fieldField = entryViewModel.GetRecordFieldFieldViewModel(nameof(BulkReplaceRequest.FieldToReplaceIn));
            fieldField.Value = fieldField.ItemsSource.First(kv => kv.Key == field);
            var oldValueField = entryViewModel.GetStringFieldFieldViewModel(nameof(BulkReplaceRequest.OldValue));
            oldValueField.Value = oldValue;
            var newValueField = entryViewModel.GetStringFieldFieldViewModel(nameof(BulkReplaceRequest.NewValue));
            newValueField.Value = newValue;
            entryViewModel.SaveButtonViewModel.Invoke();
            var completionScreen = dialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            completionScreen.CompletionDetails.LoadFormSections();
            Assert.IsFalse(completionScreen.CompletionDetails.GetEnumerableFieldViewModel(nameof(BulkReplaceResponse.ResponseItems)).GetGridRecords(false).Records.Any());
            completionScreen.CompletionDetails.CancelButtonViewModel.Invoke();
            Assert.IsFalse(parentDialog.ChildForms.Any());
        }
    }
}