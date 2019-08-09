using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    [MenuItemVisibleForDeployIntoFieldProject]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class AddPortalCodeModule : ServiceRequestModule<AddPortalCodeDialog, AddPortalCodeService, AddPortalCodeRequest, AddPortalCodeResponse, AddPortalCodeResponseItem>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddRecordsForSelectionInAddPortalRecords();
        }

        private void AddRecordsForSelectionInAddPortalRecords()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                switch (changedField)
                {
                    case nameof(AddPortalCodeRequest.PortalRecordsToExport.IncludeAll):
                        {
                            if (!revm.GetBooleanFieldFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.IncludeAll)).Value)
                            {
                                var parentForm = revm.ParentForm;
                                if (parentForm == null)
                                    throw new Exception("Cannot get parent form");
                                parentForm.LoadingViewModel.IsLoading = true;
                                revm.DoOnAsynchThread(() =>
                                {
                                    try
                                    {
                                        var recordType = revm.GetRecordTypeFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.RecordType)).Value?.Key;
                                        if (recordType == null)
                                            throw new NullReferenceException($"Error {nameof(AddPortalCodeRequest.PortalRecordsToExport.RecordType)} is null");
                                        var lookupService = revm.RecordService.LookupService;
                                        var websiteId = parentForm.GetLookupFieldFieldViewModel(nameof(AddPortalCodeRequest.WebSite)).Value?.Id;
                                        if (websiteId == null)
                                        {
                                            revm.GetBooleanFieldFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.IncludeAll)).Value = true;
                                            revm.ApplicationController.UserMessage("Please Select The Website Before Selecting Records For Inclusion");
                                            return;
                                        }

                                        var results = AddPortalCodeConfiguration.GetRecordsForConfig(recordType, lookupService, websiteId);
                                        if (!results.Any())
                                        {
                                            revm.GetBooleanFieldFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.IncludeAll)).Value = true;
                                            revm.ApplicationController.UserMessage("Sorry There No Records Of This Type Were Identified For Selection");
                                        }
                                        else
                                        {
                                            var grid = revm.GetEnumerableFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.RecordsToInclude));
                                            var nameField = lookupService.GetPrimaryField(recordType);
                                            var itemsForSelection = results
                                                .Select(r => new AddPortalCodeRequest.PortalRecordsToExport.SelectableRecordToInclude(r.Id, r.GetStringField(nameField)))
                                                .ToArray();
                                            revm.GetFieldViewModel(nameof(AddPortalCodeRequest.PortalRecordsToExport.RecordsToInclude)).ValueObject = itemsForSelection;
                                            revm.OnPropertyChanged(nameof(AddPortalCodeRequest.PortalRecordsToExport.RecordsToInclude));

                                            //okay so lets autoload the multi select
                                            //if user has selected not to inlude all, then naturally they will have to select which specific ones
                                            //so lets save a click!
                                            grid.BulkAddButton.Invoke();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        revm.ApplicationController.ThrowException(ex);
                                    }
                                    finally
                                    {
                                        parentForm.LoadingViewModel.IsLoading = false;
                                    }
                                });
                            }
                            break;
                        }
                }
            });
            this.AddOnChangeFunction(customFunction, typeof(AddPortalCodeRequest.PortalRecordsToExport));
        }
    }
}
