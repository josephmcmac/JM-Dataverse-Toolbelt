using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Extentions;
using System.Threading;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class BulkAddLookupFunction : BulkAddFunction
    {
        public override Type TargetPropertyType
        {
            get { return typeof(Lookup); }
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

                        targetTypes.Add(recordForm.FormService.GetLookupTargetType(subGridReference + "." + targetPropertyInfo.Name, GetEnumeratedType(recordForm, subGridReference).FullName, recordForm));

                        var lookupService = GetLookupService(recordForm, subGridReference);


                        try
                        {
                            recordForm.DoOnMainThread(() =>
                            {
                                var childForm = new QueryViewModel(targetTypes, lookupService, recordForm.ApplicationController, null, allowQuery: true);

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
            var lookup = GetLookupService(recordForm, subGridReference).ToLookup(selectedRow.GetRecord());
            if (gridField.GridRecords.Any(g => g.GetLookupFieldFieldViewModel(targetPropertyname).Value == lookup))
                return;
            newRecord.SetField(targetPropertyname, lookup, recordForm.RecordService);
            gridField.InsertRecord(newRecord, 0);
        }
    }
}