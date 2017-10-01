using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Extentions;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Record.Service;
using System.Threading;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class BulkAddRecordTypeFunction : BulkAddFunction
    {
        public override Type TargetPropertyType
        {
            get { return typeof(RecordType); }
        }

        public override Action GetCustomFunction(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            return () =>
            {

                    recordForm.LoadingViewModel.IsLoading = true;
                    recordForm.DoOnAsynchThread(() =>
                    {
                        try
                        {
                            Thread.Sleep(100);
                            var targetPropertyInfo = GetTargetProperty(recordForm, subGridReference);

                            var targetTypes = new List<string>();
                            targetTypes.Add(typeof(IRecordTypeMetadata).AssemblyQualifiedName);

                            var lookupService = recordForm.RecordService.GetLookupService(targetPropertyInfo.Name, GetEnumeratedType(recordForm, subGridReference).FullName, subGridReference, recordForm.GetRecord());
                            var types = lookupService
                                .GetAllRecordTypes()
                                .Select(r => lookupService.GetRecordTypeMetadata(r))
                                .Where(r => r.Searchable)
                                .OrderBy(r => r.DisplayName)
                                .ToArray();


                            var queryTypesObject = new RecordTypesObject
                            {
                                RecordTypes = types
                            };
                            var queryTypesService = new ObjectRecordService(queryTypesObject, recordForm.ApplicationController);

                            try
                            {
                                recordForm.DoOnMainThread(() =>
                                {
                                    var childForm = new QueryViewModel(targetTypes, queryTypesService, recordForm.ApplicationController, null, loadInitially: true);

                                    Load(recordForm, subGridReference, childForm);
                                });
                            }
                            catch (Exception ex)
                            {
                                recordForm.ApplicationController.ThrowException(ex);
                            }
                            finally
                            {
                                recordForm.LoadingViewModel.IsLoading = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            recordForm.ApplicationController.ThrowException(ex);
                            recordForm.LoadingViewModel.IsLoading = false;
                        }
                    });
            };
        }

        public override void AddSelectedItem(GridRowViewModel selectedRow, RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var gridField = GetObjectFormService(recordForm).GetSubGridViewModel(subGridReference);
            var targetPropertyname = GetTargetProperty(recordForm, subGridReference).Name;
            var newRecord = recordForm.RecordService.NewRecord(GetEnumeratedType(recordForm, subGridReference).AssemblyQualifiedName);

            var selectedRowrecord = selectedRow.GetRecord() as ObjectRecord;
            if (selectedRowrecord != null)
            {
                var newRecordType = new RecordType();
                newRecordType.Key = (string)selectedRowrecord.Instance.GetPropertyValue(nameof(IRecordTypeMetadata.SchemaName));
                newRecordType.Value = (string)selectedRowrecord.Instance.GetPropertyValue(nameof(IRecordTypeMetadata.DisplayName));

                newRecord.SetField(targetPropertyname, newRecordType, recordForm.RecordService);
                if (gridField.GridRecords.Any(g => g.GetRecordTypeFieldViewModel(targetPropertyname).Value == newRecordType))
                    return;
                newRecord.SetField(targetPropertyname, newRecordType, recordForm.RecordService);
                gridField.InsertRecord(newRecord, 0);
            }
        }

        public class RecordTypesObject
        {
            public IEnumerable<IRecordTypeMetadata> RecordTypes { get; set; }
        }
    }
}