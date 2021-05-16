using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.MigrateInternal
{
    [MyDescription("Migrate Records From One Record Type To Another In A Dynamics CRM Instance")]
    public class MigrateInternalModule
        : ServiceRequestModule<MigrateInternalDialog, MigrateInternalService, MigrateInternalRequest, MigrateInternalResponse, MigrateInternalResponseItem>
    {
        public override string MenuGroup => "Data Import/Export";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddGenerateMappingsWhenTypesSelected();
        }

        private void AddGenerateMappingsWhenTypesSelected()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                switch (changedField)
                {
                    case nameof(MigrateInternalRequest.MigrateInternalTypeMapping.SourceType):
                    case nameof(MigrateInternalRequest.MigrateInternalTypeMapping.TargetType):
                        {
                            if (revm.GetFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.SourceType)).ValueObject != null
                                && revm.GetFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.TargetType)).ValueObject != null)
                            {
                                var mappings = revm.GetEnumerableFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.FieldMappings));
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
            this.AddOnChangeFunction(customFunction, typeof(MigrateInternalRequest.MigrateInternalTypeMapping));
        }

        private void GenerateMappings(RecordEntryViewModelBase revm)
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
                            var sourceType = revm.GetRecordTypeFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.SourceType)).Value?.Key;
                        var targetType = revm.GetRecordTypeFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.TargetType)).Value?.Key;

                        var targetLookupService = r.RecordService.GetLookupService(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.TargetType), nameof(MigrateInternalRequest.MigrateInternalTypeMapping), nameof(MigrateInternalRequest.MigrateInternalTypeMapping), revm.GetRecord());

                        if (targetLookupService != null
                            && sourceType != null
                            && targetType != null)
                        {
                            var sourceTypeFields = targetLookupService.GetFields(sourceType);

                            var fieldMappings = new List<MigrateInternalRequest.MigrateInternalTypeMapping.MigrateInternalFieldMapping>();
                            foreach (var field in sourceTypeFields)
                            {
                                var sourceFieldLogicalName = field;
                                var sourceFieldLabel = targetLookupService.GetFieldLabel(sourceFieldLogicalName, sourceType);
                                var fieldMapping = new MigrateInternalRequest.MigrateInternalTypeMapping.MigrateInternalFieldMapping();
                                fieldMapping.SourceField = new RecordField(field, sourceFieldLabel);
                                if (targetType != null)
                                {
                                    var targetFieldResponse = GetTargetField(targetLookupService, sourceFieldLogicalName, sourceType, targetType);
                                    if (targetFieldResponse.IsMatch)
                                    {
                                        fieldMapping.TargetField = new RecordField(targetFieldResponse.LogicalName, targetLookupService.GetFieldLabel(targetFieldResponse.LogicalName, targetType));
                                    }
                                }
                                fieldMappings.Add(fieldMapping);
                            }
                            revm.GetEnumerableFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.FieldMappings)).Value = fieldMappings;
                            if (revm is ObjectEntryViewModel oevm)
                            {
                                oevm.Reload();
                            }
                            revm.OnPropertyChanged(nameof(EnumerableFieldViewModel.StringDisplay));
                        }
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

        private static GetTargetFieldResponse GetTargetField(IRecordService service, string sourceString, string sourceType, string targetType)
        {
            sourceString = sourceString?.ToLower();
            var sourceLabel = service.GetFieldLabel(sourceString, sourceType);
            var fieldMt = service.GetFieldMetadata(targetType);
            var fieldsForLabel = fieldMt.Where(f => f.DisplayName?.ToLower() == sourceLabel?.ToLower());
            if (fieldsForLabel.Count() == 1)
                return new GetTargetFieldResponse(fieldsForLabel.First().SchemaName);

            var firstUnderscore = sourceString.IndexOf("_");
            var strippedPrefixAndUnderscore = (firstUnderscore > -1
                ? sourceString.Substring(firstUnderscore + 1)
                : sourceString).Replace("_", "");

            var fieldsForName = fieldMt.Where(f => (f.SchemaName.Substring(f.SchemaName.IndexOf("_") > -1 ? f.SchemaName.IndexOf("_") + 1 : 0))?.Replace("_", "") == strippedPrefixAndUnderscore);
            if (fieldsForName.Any())
                return new GetTargetFieldResponse(fieldsForName.First().SchemaName);

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
    }
}