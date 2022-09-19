using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.ExcelImport
{
    [MyDescription("Import data from an Excel file")]
    public class ExcelImportModule
        : ServiceRequestModule<ExcelImportDialog, ExcelImportService, ExcelImportRequest, ExcelImportResponse, ExcelImportResponseItem>
    {
        public override string MenuGroup => "Import & Migrate Data";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddGenerateMappingsWhenExcelFileSelected();
        }

        private void AddGenerateMappingsWhenExcelFileSelected()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                switch(changedField)
                {
                    case nameof(ExcelImportRequest.ExcelFile):
                        {
                            if (revm.GetFieldViewModel(nameof(ExcelImportRequest.ExcelFile)).ValueObject != null)
                            {
                                var mappings = revm.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
                                if (mappings.DynamicGridViewModel != null
                                    && mappings.GridRecords != null
                                    && !mappings.GridRecords.Any())
                                {
                                    GenerateMappings(mappings.DynamicGridViewModel);
                                }
                            }
                            break;
                        }
                }
            });
            this.AddOnChangeFunction(customFunction, typeof(ExcelImportRequest));
        }

        private void AddGenerateColumnMappingsTargetTypeSelected()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                switch (changedField)
                {
                    case nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType):
                        {
                            if (revm.GetFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab)).ValueObject != null
                                && revm.GetFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType)).ValueObject != null)
                            {
                                var mappings = revm.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings));
                            }
                            break;
                        }
                }
            });
            this.AddOnChangeFunction(customFunction, typeof(ExcelImportRequest.ExcelImportTabMapping));
        }

        private static void GenerateMappings(DynamicGridViewModel g)
        {
            g.ApplicationController.DoOnAsyncThread(() =>
            {
                try
                {
                    var r = g.ParentForm;
                    if (r == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");

                    r.LoadingViewModel.LoadingMessage = "Please Wait While Generating Mappings";
                    r.LoadingViewModel.IsLoading = true;
                    try
                    {
                        var mappingsGrid = r.GetEnumerableFieldViewModel(nameof(ExcelImportRequest.Mappings));
                        //alright this one is going to create a mapping for each excel tab
                        //if the name matches with a target type then we will auto populate the target
                        var excelSourceService = r.RecordService.GetLookupService(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab), typeof(ExcelImportRequest.ExcelImportTabMapping).AssemblyQualifiedName, nameof(ExcelImportRequest.Mappings), r.GetRecord());
                        var sourceTypes = excelSourceService.GetAllRecordTypes();
                        var targetLookupService = r.RecordService.GetLookupService(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType), nameof(ExcelImportRequest.ExcelImportTabMapping), nameof(ExcelImportRequest.Mappings), r.GetRecord());
                        foreach (var sourceType in sourceTypes)
                        {
                            var sourceLabel = excelSourceService.GetDisplayName(sourceType);
                            var newRecord = r.RecordService.NewRecord(typeof(ExcelImportRequest.ExcelImportTabMapping).AssemblyQualifiedName);
                            newRecord.SetField(nameof(ExcelImportRequest.ExcelImportTabMapping.SourceTab), new RecordType(sourceType, sourceLabel), r.RecordService);
                            var sourceColumns = excelSourceService.GetFields(sourceType);
                            if (targetLookupService != null)
                            {
                                var targetTypeResponse = GetTargetType(targetLookupService, sourceLabel);
                                if (targetTypeResponse.IsMatch)
                                {
                                    newRecord.SetField(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType), new RecordType(targetTypeResponse.LogicalName, targetLookupService.GetDisplayName(targetTypeResponse.LogicalName)), r.RecordService);
                                }
                            }
                            var targetType = newRecord.GetOptionKey(nameof(ExcelImportRequest.ExcelImportTabMapping.TargetType));
                            var fieldMappings = new List<ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping>();
                            foreach (var field in sourceColumns)
                            {
                                var fieldLabel = excelSourceService.GetFieldLabel(field, sourceType);
                                var fieldMapping = new ExcelImportRequest.ExcelImportTabMapping.ExcelImportFieldMapping();
                                fieldMapping.SourceColumn = new RecordField(field, fieldLabel);
                                if(targetType != null)
                                {
                                    var targetFieldResponse = GetTargetField(targetLookupService, fieldLabel, targetType);
                                    if (targetFieldResponse.IsMatch)
                                    {
                                        fieldMapping.TargetField = new RecordField(targetFieldResponse.LogicalName, targetLookupService.GetFieldLabel(targetFieldResponse.LogicalName, targetType));
                                    }
                                }
                                fieldMappings.Add(fieldMapping);
                            }
                            newRecord.SetField(nameof(ExcelImportRequest.ExcelImportTabMapping.Mappings), fieldMappings, r.RecordService);

                            mappingsGrid.InsertRecord(newRecord, 0);
                        }
                    }
                    finally
                    {
                        r.LoadingViewModel.IsLoading = false;
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            });
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
        }

        private static GetTargetTypeResponse GetTargetType(IRecordService service, string sourceString)
        {
            sourceString = sourceString?.ToLower();
            var recordTypes = service.GetAllRecordTypes();
            var typesForLabel = recordTypes.Where(t => service.GetDisplayName(t)?.ToLower() == sourceString || service.GetCollectionName(t)?.ToLower() == sourceString);
            if (typesForLabel.Count() == 1)
                return new GetTargetTypeResponse(typesForLabel.First(), false);
            var typesForName = recordTypes.Where(t => t == sourceString);
            if (typesForName.Any())
                return new GetTargetTypeResponse(typesForName.First(), false);

            var manyToManys = service.GetManyToManyRelationships();
            var manysmatch = manyToManys.Where(mm => mm.SchemaName == sourceString);
            if(manysmatch.Any())
                return new GetTargetTypeResponse(manysmatch.First().SchemaName, false);

            return new GetTargetTypeResponse();
        }

        private class GetTargetTypeResponse
        {
            public GetTargetTypeResponse()
            {

            }

            public GetTargetTypeResponse(string logicalName, bool isRelationship)
            {
                LogicalName = logicalName;
                IsRelationship = isRelationship;
            }

            public bool IsMatch { get { return LogicalName != null; } }
            public bool IsRelationship { get; set; }
            public string LogicalName { get; set; }
        }
    }
}