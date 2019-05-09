using JosephM.Core.FieldType;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using JosephM.Xrm.Vsix.Module.UpdateAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixGeneralAssemblyTests : JosephMVsixTests
    {
        /// <summary>
        /// creates and updates a plugin assembly and verifies code runs in dyanmics
        /// </summary>
        [TestMethod]
        public void VsixGeneralAssemblyCreateUpdateAndRunPluginAssemblyTest()
        {
            //alright so this is a slightly broader example for deploying assembly
            //covering more operations updating an assembly

            var testAssemblyName = "TestAssemblyDeploy.Plugins";
            DeleteTestPluginAssembly(useAssemblyName: testAssemblyName);
            Assert.IsFalse(GetTestPluginAssemblyRecords(useAssemblyName: testAssemblyName).Any());

            var assemblyV1 = Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestAssemblyDeploy", "Version1", testAssemblyName + ".dll");
            var assemblyV2 = Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestAssemblyDeploy", "Version2", testAssemblyName + ".dll");
            var assemblyV3 = Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestAssemblyDeploy", "Version3", testAssemblyName + ".dll");
            const string testWorkflowGroup = "ScriptWorkflowGroup";
            const string inputArgumentV1 = "InputArgument1";
            const string inputArgumentV2 = "InputArgument2";

            //okay initial assembly has
            //1 * plugin 
            //1 * workflow activity with 1 input argument
            var testApplication = CreateAndLoadTestApplication<DeployAssemblyModule>(loadXrmConnection: false);

            //deploy first assembly version
            VisualStudioService.SetSelectedProjectAssembly(assemblyV1);
            var deployDialog = testApplication.NavigateToDialog<DeployAssemblyModule, DeployAssemblyDialog>();
            var deployEntryForm = testApplication.GetSubObjectEntryViewModel(deployDialog);
            var pluginGrid = deployEntryForm.GetEnumerableFieldViewModel(nameof(DeployAssemblyRequest.PluginTypes));
            //verify correct plugin types
            Assert.AreEqual(2, pluginGrid.GridRecords.Count);
            Assert.AreEqual(1, pluginGrid.GridRecords.Count(p => p.GetRecord().GetBoolField(nameof(PluginType.IsWorkflowActivity))));
            Assert.AreEqual(1, pluginGrid.GridRecords.Count(p => !p.GetRecord().GetBoolField(nameof(PluginType.IsWorkflowActivity))));
            foreach(var item in pluginGrid.GridRecords.Where(p => p.GetRecord().GetBoolField(nameof(PluginType.IsWorkflowActivity))))
            {
                //set workflow group
                item.GetStringFieldFieldViewModel(nameof(PluginType.GroupName)).Value = testWorkflowGroup;
            }
            Assert.IsTrue(deployEntryForm.Validate());
            deployEntryForm.SaveButtonViewModel.Invoke();
            //verify no errors
            var deployResponse = deployDialog.CompletionItem as DeployAssemblyResponse;
            Assert.IsFalse(deployResponse.HasError);

            //verify assembly created with correct plugin types
            var assemblyRecords = GetTestPluginAssemblyRecords(useAssemblyName: testAssemblyName);
            Assert.AreEqual(1, assemblyRecords.Count());
            foreach (var assembly in assemblyRecords)
            {
                var pluginTypes = GetPluginTypes(assembly);
                Assert.AreEqual(2, pluginTypes.Count());

                //first assembly version only has one of 2 workflow input arguments
                var argumentWorkflow = pluginTypes.First(p => p.GetStringField(Fields.plugintype_.name) == "TestAssemblyDeployWorkflowActivity1");
                Assert.AreEqual(testWorkflowGroup, argumentWorkflow.GetStringField(Fields.plugintype_.workflowactivitygroupname));
                var inputXmlField = argumentWorkflow.GetStringField(Fields.plugintype_.customworkflowactivityinfo);
                Assert.IsTrue(inputXmlField.Contains(inputArgumentV1));
                Assert.IsFalse(inputXmlField.Contains(inputArgumentV2));
            }

            //second assembly has
            //2 * plugin 
            //2 * workflow activity
            //input argument added to the workflow activity in initial deployment

            //deploy second assembly version
            VisualStudioService.SetSelectedProjectAssembly(assemblyV2);
            deployDialog = testApplication.NavigateToDialog<DeployAssemblyModule, DeployAssemblyDialog>();
            deployEntryForm = testApplication.GetSubObjectEntryViewModel(deployDialog);
            pluginGrid = deployEntryForm.GetEnumerableFieldViewModel(nameof(DeployAssemblyRequest.PluginTypes));
            //verify correct plugin types
            Assert.AreEqual(4, pluginGrid.GridRecords.Count);
            Assert.AreEqual(2, pluginGrid.GridRecords.Count(p => p.GetBooleanFieldFieldViewModel(nameof(PluginType.IsDeployed)).Value));
            Assert.AreEqual(2, pluginGrid.GridRecords.Count(p => p.GetRecord().GetBoolField(nameof(PluginType.IsWorkflowActivity))));
            Assert.AreEqual(2, pluginGrid.GridRecords.Count(p => !p.GetRecord().GetBoolField(nameof(PluginType.IsWorkflowActivity))));

            //set this to also refresh workflow arguments
            deployEntryForm.GetBooleanFieldFieldViewModel(nameof(DeployAssemblyRequest.TriggerWorkflowActivityRefreshes)).Value = true;
            foreach (var item in pluginGrid.GridRecords.Where(p => p.GetRecord().GetBoolField(nameof(PluginType.IsWorkflowActivity))))
            {
                //set workflow group
                item.GetStringFieldFieldViewModel(nameof(PluginType.GroupName)).Value = testWorkflowGroup;
            }
            Assert.IsTrue(deployEntryForm.Validate());
            deployEntryForm.SaveButtonViewModel.Invoke();
            //verify no errors
            deployResponse = deployDialog.CompletionItem as DeployAssemblyResponse;
            Assert.IsFalse(deployResponse.HasError);

            //verify assembly created with correct plugin types
            assemblyRecords = GetTestPluginAssemblyRecords(useAssemblyName: testAssemblyName);
            Assert.AreEqual(1, assemblyRecords.Count());
            foreach (var assembly in assemblyRecords)
            {
                var pluginTypes = GetPluginTypes(assembly);
                Assert.AreEqual(4, pluginTypes.Count());

                //second assembly version only has one of 2 workflow input arguments
                var argumentWorkflow = pluginTypes.First(p => p.GetStringField(Fields.plugintype_.name) == "TestAssemblyDeployWorkflowActivity1");
                Assert.AreEqual(testWorkflowGroup, argumentWorkflow.GetStringField(Fields.plugintype_.workflowactivitygroupname));
                var inputXmlField = argumentWorkflow.GetStringField(Fields.plugintype_.customworkflowactivityinfo);
                Assert.IsTrue(inputXmlField.Contains(inputArgumentV1));
                Assert.IsTrue(inputXmlField.Contains(inputArgumentV2));
            }

            //next assembly has added an error thrown for account plugin
            //so we need to create a plugin trigger for it on create account
            //update the assembly
            //then verify the error is thrown wher creating an account

            //add plugin trigger for peoperation create account
            testApplication.AddModule<ManagePluginTriggersModule>();
            var triggersDialog = testApplication.NavigateToDialog<ManagePluginTriggersModule, ManagePluginTriggersDialog>();
            var triggersEntryForm = testApplication.GetSubObjectEntryViewModel(triggersDialog);
            var triggersGrid = triggersEntryForm.GetEnumerableFieldViewModel(nameof(ManagePluginTriggersRequest.Triggers));

            //add trigger to the grid
            triggersGrid.DynamicGridViewModel.AddRowButton.Invoke();
            Assert.IsTrue(triggersEntryForm.ChildForms.Any());
            var triggerEntry = testApplication.GetSubObjectEntryViewModel(triggersEntryForm);
            triggerEntry.GetLookupFieldFieldViewModel(nameof(PluginTrigger.Plugin)).SelectedItem = triggerEntry.GetLookupFieldFieldViewModel(nameof(PluginTrigger.Plugin)).ItemsSource.First(p => p.Name == "TestAssemblyDeployPluginRegistration");
            triggerEntry.GetLookupFieldFieldViewModel(nameof(PluginTrigger.Message)).SelectedItem =  triggerEntry.GetLookupFieldFieldViewModel(nameof(PluginTrigger.Message)).ItemsSource.First(p => p.Name == "Create");
            triggerEntry.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Stage)).Value = triggerEntry.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Stage)).ItemsSource.First(p => p.Key == PicklistOption.EnumToPicklistOption(PluginTrigger.PluginStage.PreOperationEvent).Key);
            triggerEntry.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Mode)).Value = triggerEntry.GetPicklistFieldFieldViewModel(nameof(PluginTrigger.Mode)).ItemsSource.First(p => p.Key == PicklistOption.EnumToPicklistOption(PluginTrigger.PluginMode.Synch).Key);
            triggerEntry.GetRecordTypeFieldViewModel(nameof(PluginTrigger.RecordType)).Value = triggerEntry.GetRecordTypeFieldViewModel(nameof(PluginTrigger.RecordType)).ItemsSource.First(p => p.Key == Entities.account);
            Assert.IsTrue(triggerEntry.Validate());
            triggerEntry.SaveButtonViewModel.Invoke();
            Assert.IsFalse(triggersEntryForm.ChildForms.Any());

            //save
            Assert.IsTrue(triggersEntryForm.Validate());
            triggersEntryForm.SaveButtonViewModel.Invoke();
            //verify no errors
            var triggersResponse = triggersDialog.CompletionItem as ManagePluginTriggersResponse;
            Assert.IsFalse(triggersResponse.HasError);

            //verify create account as we havewnt deployed the assembly throwing an error yet
            var account = CreateAccount();

            //update the assembly to the one throwing error for account create
            var updateAssemblyModule = testApplication.AddModule<UpdateAssemblyModule>();
            VisualStudioService.SetSelectedProjectAssembly(assemblyV3);
            updateAssemblyModule.DialogCommand();
            var updateAssemblyDialog = testApplication.GetNavigatedDialog<UpdateAssemblyDialog>();
            Assert.IsNull(updateAssemblyDialog.FatalException);
            var updateResponse = updateAssemblyDialog.CompletionItem as UpdateAssemblyResponse;
            Assert.IsNotNull(updateResponse.CompletionMessage);

            //verify error thrown now when creating account
            WaitTillTrue(() =>
            {
                var errorThrown = false;
                try
                {
                    account = CreateAccount();
                }
                catch(Exception)
                {
                    errorThrown = true;
                }
                return errorThrown;
            });

            //update the assembly to the one doesnt throw an error
            VisualStudioService.SetSelectedProjectAssembly(assemblyV2);
            updateAssemblyModule.DialogCommand();
            updateAssemblyDialog = testApplication.GetNavigatedDialog<UpdateAssemblyDialog>();
            Assert.IsNull(updateAssemblyDialog.FatalException);
            updateResponse = updateAssemblyDialog.CompletionItem as UpdateAssemblyResponse;
            Assert.IsNotNull(updateResponse.CompletionMessage);

            //verify error not thrown now when creating account
            WaitTillTrue(() =>
            {
                var errorThrown = false;
                try
                {
                    account = CreateAccount();
                }
                catch (Exception)
                {
                    errorThrown = true;
                }
                return !errorThrown;
            });

            //delete the assembly
            //if we dont delete it next time the script is run there are sometimes caching issues
            //due to deleting the creating the assembly in quick succession
            DeleteTestPluginAssembly(useAssemblyName: testAssemblyName);
            Assert.IsFalse(GetTestPluginAssemblyRecords(useAssemblyName: testAssemblyName).Any());
        }
    }
}
