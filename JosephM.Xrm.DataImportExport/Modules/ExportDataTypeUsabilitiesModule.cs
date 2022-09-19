using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Xrm.DataImportExport.XmlImport;
using System;

namespace JosephM.Xrm.DataImportExport.Modules
{
    public class ExportDataTypeUsabilitiesModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            AutoOpenSelections();
        }

        private void AutoOpenSelections()
        {
            var customFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                try
                {
                    if (revm is GridRowViewModel)
                    {
                        switch (changedField)
                        {
                            case nameof(ExportRecordType.IncludeAllFields):
                                {
                                    if (!revm.GetBooleanFieldFieldViewModel(nameof(ExportRecordType.IncludeAllFields)).Value ?? false)
                                    {
                                       var fieldsIncludedViewModel = revm.GetEnumerableFieldViewModel(nameof(ExportRecordType.IncludeOnlyTheseFields));
                                        if (fieldsIncludedViewModel.Value  == null || !fieldsIncludedViewModel.Value.GetEnumerator().MoveNext())
                                        {
                                            fieldsIncludedViewModel.BulkAddButton?.Invoke();
                                        }
                                    }
                                    break;
                                }
                            case nameof(ExportRecordType.Type):
                                {
                                    if (revm.GetPicklistFieldFieldViewModel(nameof(ExportRecordType.Type)).Value == PicklistOption.EnumToPicklistOption(ExportType.SpecificRecords))
                                    {
                                        var recordsIncludedViewModel = revm.GetEnumerableFieldViewModel(nameof(ExportRecordType.SpecificRecordsToExport));
                                        if (recordsIncludedViewModel.Value == null || !recordsIncludedViewModel.Value.GetEnumerator().MoveNext())
                                        {
                                            recordsIncludedViewModel.BulkAddButton?.Invoke();
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    revm.ApplicationController.ThrowException(ex);
                }
            });
            this.AddOnChangeFunction(customFunction, typeof(ExportRecordType));
        }
    }
}