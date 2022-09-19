﻿using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using System;
using System.Linq;

namespace JosephM.Xrm.SqlImport
{
    [MyDescription("Import data from an SQL connection")]
    public class SqlImportModule
        : ServiceRequestModule<SqlImportDialog, SqlImportService, SqlImportRequest, SqlImportResponse, SqlImportResponseItem>
    {
        public override string MenuGroup => "Import & Migrate Data";

        public override void RegisterTypes()
        {
            base.RegisterTypes();
            AddGenerateMappingsButtonOnColumnMappingsForm();
            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(SqlImportResponse.Connection))}"
                , (r) =>
                {
                    try
                    {
                        var connection = r.GetRecord().GetField(nameof(SqlImportResponse.Connection)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(SqlImportResponse.Connection)) != null)
                , typeof(SqlImportResponse));
        }

        private void AddGenerateMappingsButtonOnColumnMappingsForm()
        {
            this.AddCustomGridFunction(new CustomGridFunction("CREATEMAPPINGS", "Create Mappings", GenerateMappings, visibleFunction: (g) => true)
            , typeof(SqlImportRequest.SqlImportTableMapping.SqlImportFieldMapping));
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
                        var mappingsGrid = r.GetEnumerableFieldViewModel(nameof(SqlImportRequest.SqlImportTableMapping.Mappings));
                        var sourceType = r.GetRecord().GetOptionKey(nameof(SqlImportRequest.SqlImportTableMapping.SourceTable));
                        if (string.IsNullOrWhiteSpace(sourceType))
                            throw new NullReferenceException($"{nameof(SqlImportRequest.SqlImportTableMapping.SourceTable)} Is Required For This Function");
                        var targetType = r.GetRecord().GetOptionKey(nameof(SqlImportRequest.SqlImportTableMapping.TargetType));
                        if (string.IsNullOrWhiteSpace(targetType))
                            throw new NullReferenceException($"{nameof(SqlImportRequest.SqlImportTableMapping.TargetType)} Is Required For This Function");
                        //alright this one is going to create a mapping for each excel tab
                        //if the name matches with a target type then we will auto populate the target
                        var sourceService = r.RecordService.GetLookupService(nameof(SqlImportRequest.SqlImportTableMapping.SourceTable), typeof(SqlImportRequest.SqlImportTableMapping).AssemblyQualifiedName, nameof(SqlImportRequest.Mappings), r.GetRecord());
                        var targetLookupService = r.RecordService.GetLookupService(nameof(SqlImportRequest.SqlImportTableMapping.TargetType), nameof(SqlImportRequest.SqlImportTableMapping), nameof(SqlImportRequest.Mappings), r.GetRecord());
                        foreach (var sourceColumn in sourceService.GetFields(sourceType))
                        {
                            var fieldLabel = sourceService.GetFieldLabel(sourceColumn, sourceType);
                            var fieldMapping = new SqlImportRequest.SqlImportTableMapping.SqlImportFieldMapping();
                            fieldMapping.SourceColumn = new RecordField(sourceColumn, fieldLabel);
                            if (targetType != null)
                            {
                                var targetFieldResponse = GetTargetField(targetLookupService, fieldLabel, targetType);
                                if (targetFieldResponse.IsMatch)
                                {
                                    fieldMapping.TargetField = new RecordField(targetFieldResponse.LogicalName, targetLookupService.GetFieldLabel(targetFieldResponse.LogicalName, targetType));
                                }
                            }
                            mappingsGrid.InsertRecord(new ObjectRecord(fieldMapping), 0);
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
    }
}