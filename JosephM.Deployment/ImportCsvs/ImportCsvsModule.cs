using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.Service;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.ImportCsvs
{
    [MyDescription("Import Records Defined In CSV Files Into A CRM Instance")]
    public class ImportCsvsModule
        : ServiceRequestModule<ImportCsvsDialog, ImportCsvsService, ImportCsvsRequest, ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public override string MenuGroup => "Data Import/Export";

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
            var customFormFunction = new CustomFormFunction("DOWNLOADCSVTEMPLATES", "Create CSV Templates", DownloadTemplates, (re) => { return true; });
            this.AddCustomFormFunction(customFormFunction, typeof(ImportCsvsRequest));
            AddProductTypesToCsvgenerationGrid();
        }

        private void AddProductTypesToCsvgenerationGrid()
        {
            var customFormFunction = new CustomGridFunction("ADDPRODUCTTYPES", "Add Product Types", (DynamicGridViewModel g) =>
            {
                try
                {
                    var r = g.ParentForm;
                    if (r == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");
                    var typesGrid = r.GetEnumerableFieldViewModel(nameof(GenerateTemplatesRequest.CsvsToGenerate));
                    var typesToAdd = new Dictionary<string, IEnumerable<string>>()
                    {
                        {  Entities.product, new [] { Fields.product_.name, Fields.product_.productnumber, Fields.product_.defaultuomid, Fields.product_.defaultuomscheduleid, Fields.product_.quantitydecimal, Fields.product_.statecode, Fields.product_.statuscode } },
                        {  Entities.pricelevel, new [] { Fields.pricelevel_.name } },
                        {  Entities.productpricelevel, new [] { Fields.productpricelevel_.pricelevelid, Fields.productpricelevel_.productid, Fields.productpricelevel_.uomscheduleid, Fields.productpricelevel_.uomid, Fields.productpricelevel_.amount, Fields.productpricelevel_.quantitysellingcode, Fields.productpricelevel_.pricingmethodcode } },
                        {  Entities.uom, new [] { Fields.uom_.name, Fields.uom_.uomscheduleid, Fields.uom_.quantity, Fields.uom_.baseuom } },
                        {  Entities.uomschedule, new [] { Fields.uomschedule_.name, Fields.uomschedule_.baseuomname } }
                    };
                    var typesGridService = typesGrid.GetRecordService();
                    foreach (var item in typesToAdd.Reverse())
                    {
                        var newRecord = typesGridService.NewRecord(typeof(GenerateTemplateConfiguration).AssemblyQualifiedName);
                        newRecord.SetField(nameof(GenerateTemplateConfiguration.RecordType), new RecordType(item.Key, item.Key), typesGridService);
                        newRecord.SetField(nameof(GenerateTemplateConfiguration.AllFields), false, typesGridService);
                        newRecord.SetField(nameof(GenerateTemplateConfiguration.FieldsToInclude), item.Value.Select(f => new FieldSetting() { RecordField = new RecordField(f, f) } ), typesGridService);
                        typesGrid.InsertRecord(newRecord, 0);
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            }, visibleFunction: (g) => true);
            this.AddCustomGridFunction(customFormFunction, typeof(GenerateTemplateConfiguration));
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

                            var csvText = string.Join(",", columnHeadings.OrderBy(s => s));
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