using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.InstanceComparer.AddToSolution;
using JosephM.Record.Extentions;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.InstanceComparer.Test
{
    [TestClass]
    public class InstanceComparerModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void InstanceComparerModuleTest()
        {
            //this just compares an instance against itself
            //rather than actually comparing 2 instances
            DeleteAll(Entities.jmcg_testentity);
            DeleteAll(Entities.account);
            var t1 = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, "Blah 1" }
            });
            var a1 = CreateTestRecord(Entities.account, new Dictionary<string, object>
            {
                { Fields.account_.name, "Blah 1" }
            });
            XrmService.Associate(Relationships.jmcg_testentity_.jmcg_testentity_account.Name, Fields.jmcg_testentity_.jmcg_testentityid, t1.Id, Fields.account_.accountid, a1.Id);

            RecreatePortalData();

            var request = new InstanceComparerRequest();
            request.ConnectionOne = GetSavedXrmRecordConfiguration();
            request.ConnectionTwo = GetSavedXrmRecordConfiguration();
            request.DataComparisons = new[]
            {
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.jmcg_testentity, Entities.jmcg_testentity)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.account, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_entityform, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_entityformmetadata, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webfile, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webpage, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webpageaccesscontrolrule, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webrole, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_websitelanguage, Entities.account)},
            };
            foreach(var prop in request.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(bool))
                    request.SetPropertyValue(prop.Name, true);
            }
            request.DisplaySavedSettingFields = false;

            var application = CreateAndLoadTestApplication<InstanceComparerModule>();
            var response = application.NavigateAndProcessDialog<InstanceComparerModule, InstanceComparerDialog, InstanceComparerResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsFalse(response.AreDifferences);
        }

        [TestMethod]
        public void InstanceComparerAddToSolutionTest()
        {
            //okay this does an instance campare against an alt organisation
            //so there are actually differences which we will spawn the add to solution dialog for
            var request = new InstanceComparerRequest();
            request.ConnectionOne = GetSavedXrmRecordConfiguration();
            request.ConnectionTwo = GetAltSavedXrmRecordConfiguration();

            foreach (var prop in request.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(bool))
                    request.SetPropertyValue(prop.Name, true);
            }
            request.DisplaySavedSettingFields = false;
            request.AllTypesForEntityMetadata = false;
            request.Data = false;
            request.EntityTypeComparisons = new[]
            {
                new InstanceComparerRequest.InstanceCompareTypeCompare()
                {
                     RecordType = new RecordType(Entities.account, Entities.account)
                }
            };

            var testScriptSolution = ReCreateTestSolution();

            var application = CreateAndLoadTestApplication<InstanceComparerModule>();
            var completionViewModel = application.NavigateAndProcessDialogGetResponseViewModel<InstanceComparerModule, InstanceComparerDialog>(request);

            //okay so lets hope now we have a completion screen with various different components
            var response = (InstanceComparerResponse)completionViewModel.GetObject();

            //in the summary grid spawn the add to solution dialog for connection 1
            var summaryGrid = completionViewModel.GetEnumerableFieldViewModel(nameof(InstanceComparerResponse.Summary));
            var addToSolutionButton1 = summaryGrid.DynamicGridViewModel.GetButton("ADDTOSOLUTIONC1");
            addToSolutionButton1.Invoke();

            //load the entry form
            var addToSolutionDialog = completionViewModel.ChildForms.First() as AddToSolutionDialog;
            addToSolutionDialog.LoadDialog();
            var addToSolutionEntry = addToSolutionDialog.Controller.UiItems.First() as ObjectEntryViewModel;
            Assert.IsNotNull(addToSolutionEntry);
            addToSolutionEntry.LoadFormSections();

            //set to add to the test script solution we created
            addToSolutionEntry.GetLookupFieldFieldViewModel(nameof(AddToSolutionRequest.SolutionAddTo)).Value = testScriptSolution.ToLookup();

            //in th tpes grid
            var typesGrid = addToSolutionEntry.GetEnumerableFieldViewModel(nameof(AddToSolutionRequest.Items));
            Assert.IsTrue(typesGrid.GridRecords.Any());
            //select to add all component types
            foreach (var row in typesGrid.GridRecords)
            {
                row.GetBooleanFieldFieldViewModel(nameof(AddToSolutionRequest.AddToSolutionComponent.Selected)).Value = true;
            }
            //okay for one that has multiuple results lets do the multi select option (rather than adding all items)
            var firstOneWithMoreThanOneResult = typesGrid.GridRecords.First(r => r.GetIntegerFieldFieldViewModel(nameof(AddToSolutionRequest.AddToSolutionComponent.Count)).Value > 1);
            firstOneWithMoreThanOneResult.GetBooleanFieldFieldViewModel(nameof(AddToSolutionRequest.AddToSolutionComponent.AddAllItems)).Value = false;
            var selectionField = firstOneWithMoreThanOneResult.GetEnumerableFieldViewModel(nameof(AddToSolutionRequest.AddToSolutionComponent.ItemsSelection));

            //verify cuurently no selected items
            Assert.IsTrue(string.IsNullOrWhiteSpace(selectionField.StringDisplay));
            //invoke to select items
            selectionField.BulkAddButton.Invoke();
            var selection = addToSolutionEntry.ChildForms.First() as MultiSelectDialogViewModel<PicklistOption>;
            //select one
            selection.ItemsSource.First().Select = true;
            //and apply
            selection.ApplyButtonViewModel.Invoke();
            //now verify multi select closes and now an item selected
            Assert.IsFalse(addToSolutionEntry.ChildForms.Any());
            Assert.IsTrue(!string.IsNullOrWhiteSpace(selectionField.StringDisplay));
            //okay trigger to do the add to solution for our entered details
            addToSolutionEntry.SaveButtonViewModel.Invoke();
            var addToSolutionCompletion = addToSolutionDialog.Controller.UiItems.First() as CompletionScreenViewModel;
            addToSolutionCompletion.CloseButton.Invoke();
            //verify after completion goes back to the differences response summary
            Assert.IsFalse(completionViewModel.ChildForms.Any());

            var expectedCountOfComponents = typesGrid.GridRecords.Sum(g => g.GetIntegerFieldFieldViewModel(nameof(AddToSolutionRequest.AddToSolutionComponent.Count)).Value)
                - firstOneWithMoreThanOneResult.GetIntegerFieldFieldViewModel(nameof(AddToSolutionRequest.AddToSolutionComponent.Count)).Value
                + 1;

            var solutionComponents = response.ServiceOne.GetLinkedRecords(Entities.solutioncomponent, Entities.solution, Fields.solutioncomponent_.solutionid, testScriptSolution.Id);
            Assert.AreEqual(expectedCountOfComponents, solutionComponents.Count());
        }
    }
}
