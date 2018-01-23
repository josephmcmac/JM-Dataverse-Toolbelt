#region

using System;
using System.Linq;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Core.Attributes;
using JosephM.Core.Utility;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Record.Extentions;
using JosephM.Record.Service;

#endregion

namespace JosephM.Deployment.ImportCsvs
{
    [MyDescription("Import Records Defined In CSV Files Into A CRM Instance")]
    public class ImportCsvsModule
        : ServiceRequestModule<ImportCsvsDialog, ImportCsvsService, ImportCsvsRequest, ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public override string MenuGroup => "Deployment";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddDownloadTemplateFormFunction();
        }

        /// <summary>
        /// add a button on the import csv form to download csv templates
        /// </summary>
        private void AddDownloadTemplateFormFunction()
        {
            var customFormFunction = new CustomFormFunction("DOWNLOADCSVTEMPLATES", "Download Templates", DownloadTemplates, (re) => { return true; });
            this.AddCustomFormFunction(customFormFunction, typeof(ImportCsvsRequest));
        }

        private void DownloadTemplates(RecordEntryFormViewModel viewModel)
        {
            //okay so something to generate one or more csv files with column headings
            //think will just create a child form entry, generate on save then return to the dialog form
            try
            {
                if (viewModel is ObjectEntryViewModel)
                {
                    var oevm = viewModel as ObjectEntryViewModel;

                    var templatesRequest = new GenerateTemplatesRequest();

                    //this is the save function after entering the csvs to generate
                    Action createTemplatesAndReturn = () =>
                    {
                        var serviceConnection = viewModel.RecordService.LookupService;
                        //loop through each csv entered and create
                        foreach(var config in templatesRequest.CsvsToGenerate)
                        {
                            var recordType = config.RecordType.Key;
                            var fieldsInEntity = serviceConnection.GetFields(recordType).ToArray();
                            var fieldsToInclude = config.AllFields
                            ? fieldsInEntity
                                .Where(f =>
                                {
                                    var mt = serviceConnection.GetFieldMetadata(f, recordType);
                                    return mt.Createable || mt.Writeable;
                                }).ToArray()
                                : config.FieldsToInclude.Select(f => f.RecordField.Key).Intersect(fieldsInEntity).ToArray();

                            var columnHeadings = templatesRequest.UseSchemaNames ? fieldsToInclude : fieldsToInclude.Select(f => serviceConnection.GetFieldLabel(f, recordType)).ToArray();
                            var csvText = string.Join(",", columnHeadings);
                            var fileNameNoExt = templatesRequest.UseSchemaNames ? recordType : serviceConnection.GetCollectionName(recordType);
                            FileUtility.WriteToFile(templatesRequest.FolderToSaveInto.FolderPath, fileNameNoExt + ".csv", csvText);
                        }
                        viewModel.ApplicationController.StartProcess("explorer", templatesRequest.FolderToSaveInto.FolderPath);
                        //reload the form and notify
                        viewModel.ClearChildForms();
                        viewModel.LoadCustomFunctions();
                    };

                    //load the entry form
                    var os = new ObjectRecordService(templatesRequest, viewModel.RecordService.LookupService, null, viewModel.ApplicationController, null);
                    var ofs = new ObjectFormService(templatesRequest, os, null);
                    var fc = new FormController(os, ofs, viewModel.ApplicationController);

                    var vm = new ObjectEntryViewModel(createTemplatesAndReturn, () => viewModel.ClearChildForms(), templatesRequest, fc);
                    viewModel.LoadChildForm(vm);
                }
            }
            catch (Exception ex)
            {
                ApplicationController.ThrowException(ex);
            }
        }
    }
}