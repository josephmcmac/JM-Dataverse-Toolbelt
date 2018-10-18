using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
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
            AddProductTypesToCsvgenerationGrid();
            AddGenerateMappingsWhenCsvFileSelected();
            AddLoadFolderButton();
        }

        /// <summary>
        /// add a button on the import csv form to download csv templates
        /// </summary>
        private void AddDownloadTemplateFormFunction()
        {
            var customFormFunction = new CustomFormFunction("DOWNLOADCSVTEMPLATES", "Create CSV Templates", DownloadTemplates, (re) => { return true; });
            this.AddCustomFormFunction(customFormFunction, typeof(ImportCsvsRequest));
        }

        private void AddLoadFolderButton()
        {
            var customFormFunction = new CustomGridFunction("LOADFOLDER", "Load Folder", (DynamicGridViewModel g) =>
            {
                try
                {
                    var r = g.ParentForm;
                    if (r == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");

                    var folder = g.ApplicationController.GetSaveFolderName();
                    if (!string.IsNullOrWhiteSpace(folder))
                    {
                        var csvFiles = FileUtility.GetFiles(folder).Where(f => f.EndsWith(".csv"));

                        var mappingGrid = r.GetEnumerableFieldViewModel(nameof(ImportCsvsRequest.CsvsToImport));

                        foreach (var csv in csvFiles)
                        {
                            var newRecord = r.RecordService.NewRecord(typeof(ImportCsvsRequest.CsvToImport).AssemblyQualifiedName);
                            newRecord.SetField(nameof(ImportCsvsRequest.CsvToImport.SourceCsv), new FileReference(csv), r.RecordService);
                            mappingGrid.InsertRecord(newRecord, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            }, visibleFunction: (g) => true);
            this.AddCustomGridFunction(customFormFunction, typeof(ImportCsvsRequest.CsvToImport));
        }

        private void AddGenerateMappingsWhenCsvFileSelected()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                switch (changedField)
                {
                    case nameof(ImportCsvsRequest.CsvToImport.SourceCsv):
                        {
                            if (revm.GetFieldViewModel(nameof(ImportCsvsRequest.CsvToImport.SourceCsv)).ValueObject != null)
                            {
                                var mappings = revm.GetEnumerableFieldViewModel(nameof(ImportCsvsRequest.CsvToImport.Mappings));
                                if (mappings.Enumerable == null
                                    || !mappings.Enumerable.GetEnumerator().MoveNext())
                                {
                                    GenerateMappings(revm);
                                }
                            }
                            break;
                        }
                }
            });
            this.AddOnChangeFunction(customFunction, typeof(ImportCsvsRequest.CsvToImport));
        }

        private void GenerateMappings(RecordEntryViewModelBase revm)
        {
            if (revm is GridRowViewModel)
            {
                revm.ApplicationController.DoOnAsyncThread(() =>
                {
                    try
                    {
                        var r = revm.ParentForm;
                        if (r == null)
                            throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");

                        r.LoadingViewModel.LoadingMessage = "Please Wait While Generating Mappings";
                        r.LoadingViewModel.IsLoading = true;
                        try
                        {
                            //if the name matches with a target type then we will auto populate the target
                            var csvSourceService = r.RecordService.GetLookupService(nameof(ImportCsvsRequest.CsvToImport.SourceType), typeof(ImportCsvsRequest.CsvToImport).AssemblyQualifiedName, nameof(ImportCsvsRequest.CsvsToImport), revm.GetRecord());
                            var sourceType = csvSourceService.GetAllRecordTypes().First();
                            revm.GetRecord().SetField(nameof(ImportCsvsRequest.CsvToImport.SourceType), new RecordType(sourceType, sourceType), revm.RecordService);
                            var targetLookupService = r.RecordService.GetLookupService(nameof(ImportCsvsRequest.CsvToImport.TargetType), nameof(ImportCsvsRequest.CsvToImport), nameof(ImportCsvsRequest.CsvsToImport), revm.GetRecord());
                            var sourceLabel = csvSourceService.GetDisplayName(sourceType);
                            var sourceColumns = csvSourceService.GetFields(sourceType);
                            if (targetLookupService != null)
                            {
                                var targetTypeResponse = GetTargetType(targetLookupService, sourceLabel);
                                if (targetTypeResponse.IsMatch)
                                {
                                    revm.GetRecordTypeFieldViewModel(nameof(ImportCsvsRequest.CsvToImport.TargetType)).Value = new RecordType(targetTypeResponse.LogicalName, targetTypeResponse.DisplayLabel);
                                }
                            }
                            var targetType = revm.GetRecordTypeFieldViewModel(nameof(ImportCsvsRequest.CsvToImport.TargetType)).Value;
                            var fieldMappings = new List<ImportCsvsRequest.CsvToImport.CsvImportFieldMapping>();
                            foreach (var field in sourceColumns)
                            {
                                var fieldLabel = csvSourceService.GetFieldLabel(field, sourceType);
                                var fieldMapping = new ImportCsvsRequest.CsvToImport.CsvImportFieldMapping();
                                fieldMapping.SourceColumn = new RecordField(field, fieldLabel);
                                if (targetType != null)
                                {
                                    var targetFieldResponse = GetTargetField(targetLookupService, fieldLabel, targetType.Key);
                                    if (targetFieldResponse.IsMatch)
                                    {
                                        fieldMapping.TargetField = new RecordField(targetFieldResponse.LogicalName, targetLookupService.GetFieldLabel(targetFieldResponse.LogicalName, targetType.Key));
                                    }
                                }
                                fieldMappings.Add(fieldMapping);
                            }
                            revm.GetEnumerableFieldViewModel(nameof(ImportCsvsRequest.CsvToImport.Mappings)).Value = fieldMappings;
                            revm.OnPropertyChanged(nameof(EnumerableFieldViewModel.StringDisplay));
                        }
                        finally
                        {
                            r.LoadingViewModel.IsLoading = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        revm.ApplicationController.ThrowException(ex);
                    }
                });
            }
        }

        private static GetTargetFieldResponse GetTargetField(IRecordService service, string sourceString, string targetType)
        {
            sourceString = sourceString?.ToLower();
            var fieldMt = service.GetFieldMetadata(targetType);
            var fieldsForLabel = fieldMt.Where(f => f.DisplayName.ToLower() == sourceString);
            if (fieldsForLabel.Count() == 1)
                return new GetTargetFieldResponse(fieldsForLabel.First().SchemaName);
            var typesForName = fieldMt.Where(f => f.SchemaName == sourceString);
            if (typesForName.Any())
                return new GetTargetFieldResponse(typesForName.First().SchemaName);

            return new GetTargetFieldResponse();
        }

        private class GetTargetFieldResponse
        {
            public GetTargetFieldResponse()
            {

            }

            public GetTargetFieldResponse(string logicalName)
            {
                LogicalName = logicalName;
            }

            public bool IsMatch { get { return LogicalName != null; } }
            public string LogicalName { get; set; }
            public string Display { get; set; }
        }

        private static GetTargetTypeResponse GetTargetType(IRecordService service, string sourceString)
        {
            sourceString = sourceString?.ToLower();
            var recordTypes = service.GetAllRecordTypes();
            var typesForLabel = recordTypes.Where(t => service.GetDisplayName(t)?.ToLower() == sourceString || service.GetCollectionName(t)?.ToLower() == sourceString);
            if (typesForLabel.Count() == 1)
                return new GetTargetTypeResponse(typesForLabel.First(), false, service.GetDisplayName(typesForLabel.First()));
            var typesForName = recordTypes.Where(t => t == sourceString);
            if (typesForName.Any())
                return new GetTargetTypeResponse(typesForName.First(), false, service.GetDisplayName(typesForName.First()));

            var manyToManys = service.GetManyToManyRelationships();
            var manysmatch = manyToManys.Where(mm => mm.SchemaName == sourceString);
            if (manysmatch.Any())
                return new GetTargetTypeResponse(manysmatch.First().SchemaName, false, manysmatch.First().PicklistDisplay);

            return new GetTargetTypeResponse();
        }

        private class GetTargetTypeResponse
        {
            public GetTargetTypeResponse()
            {

            }

            public GetTargetTypeResponse(string logicalName, bool isRelationship, string displayLabel)
            {
                LogicalName = logicalName;
                IsRelationship = isRelationship;
                DisplayLabel = displayLabel;
            }

            public bool IsMatch { get { return LogicalName != null; } }
            public bool IsRelationship { get; set; }
            public string LogicalName { get; set; }
            public string DisplayLabel { get; set; }
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