using JosephM.Application.Application;
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

namespace JosephM.Xrm.MigrateInternal
{
    [MyDescription("Migrate data inside instance to other fields or record types")]
    public class MigrateInternalModule
        : ServiceRequestModule<MigrateInternalDialog, MigrateInternalService, MigrateInternalRequest, MigrateInternalResponse, MigrateInternalResponseItem>
    {
        public override string MenuGroup => "Import & Migrate Data";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            var configManager = Resolve<ISettingsManager>();
            configManager.ProcessNamespaceChange(GetType().Namespace, "JosephM.Deployment.MigrateInternal");
            AddGenerateMappingsButton();
            AddFieldMapsButtons();
        }

        public void AddFieldMapsButtons()
        {
            Action<bool, DynamicGridViewModel> clearMaps = (onlyClearUnmapped, g) =>
            {
                try
                {
                    var r = g.ParentForm;
                    if (r == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");

                    var enumerableField = r.GetEnumerableFieldViewModel(g.ReferenceName);
                    foreach (var row in enumerableField.GridRecords.ToArray())
                    {
                        var sourceField = row.GetFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.MigrateInternalFieldMapping.SourceField));
                        var targetField = row.GetFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.MigrateInternalFieldMapping.TargetField));
                        if (!onlyClearUnmapped
                            || (sourceField.ValueObject == null || targetField.ValueObject == null))
                        {
                            row.DeleteRow();
                        }
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            };

            this.AddCustomGridFunction(new CustomGridFunction("CLEARUNMAPPED", "Clear Unmapped", (g) =>
            {
                clearMaps(true, g);
                g.RefreshGridButtons();
            }, (g) =>
            {
                return g.GridRecords != null && g.GridRecords.Any();
            }), typeof(MigrateInternalRequest.MigrateInternalTypeMapping.MigrateInternalFieldMapping));
            this.AddCustomGridFunction(new CustomGridFunction("CLEARALLMAPSMAPPED", "Clear All Maps", (g) =>
            {
                clearMaps(false, g);
                g.RefreshGridButtons();
            }, (g) =>
            {
                return g.GridRecords != null && g.GridRecords.Any();
            }), typeof(MigrateInternalRequest.MigrateInternalTypeMapping.MigrateInternalFieldMapping));
        }

        public void AddGenerateMappingsButton()
        {
            Action<bool, DynamicGridViewModel> generateMaps = (onlyClearUnmapped, g) =>
            {
                try
                {
                    var parentForm = g.ParentForm;
                    if (parentForm == null)
                        throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");

                    var objectRecord = parentForm.GetRecord();
                    var sourceType = objectRecord.GetField(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.SourceType)) as RecordType;
                    var targetType = objectRecord.GetField(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.TargetType)) as RecordType;

                    if (sourceType?.Key != null
                        && targetType?.Key != null)
                    {
                        var mappings = parentForm.GetEnumerableFieldViewModel(nameof(MigrateInternalRequest.MigrateInternalTypeMapping.FieldMappings));
                        if (mappings.Enumerable == null
                            || !mappings.Enumerable.GetEnumerator().MoveNext())
                        {
                            GenerateMappings(parentForm, sourceType.Key, targetType.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            };

            this.AddCustomGridFunction(new CustomGridFunction("GENERATEMAPS", "Generate Maps", (g) =>
            {
                generateMaps(true, g);
                g.RefreshGridButtons();
            }, (g) =>
            {
                return g.GridRecords != null && !g.GridRecords.Any();
            }), typeof(MigrateInternalRequest.MigrateInternalTypeMapping.MigrateInternalFieldMapping));
        }

        private void GenerateMappings(RecordEntryViewModelBase revm, string sourceType, string targetType)
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