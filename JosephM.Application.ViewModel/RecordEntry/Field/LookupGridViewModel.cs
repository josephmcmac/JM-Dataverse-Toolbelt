using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class LookupGridViewModel : ViewModelBase
    {
        public LookupGridViewModel(IReferenceFieldViewModel referenceField,
            Action<IRecord> onRecordSelected)
            : base(referenceField.RecordEntryViewModel.ApplicationController)
        {
            OnRecordSelected = onRecordSelected;

            Func<bool, GetGridRecordsResponse> getGridRecords = (ignorePages) =>
                {
                    var query = new QueryDefinition(referenceField.RecordTypeToLookup);
                    query.IsQuickFind = true;
                    query.QuickFindText = referenceField.EnteredText;
                    if (!string.IsNullOrWhiteSpace(referenceField.EnteredText))
                    {
                        var quickFindFields = DynamicGridViewModel.RecordService.GetStringQuickfindFields(referenceField.RecordTypeToLookup);
                        query.RootFilter.ConditionOperator = FilterOperator.Or;
                        query.RootFilter.Conditions.AddRange(quickFindFields.Select(f => new Condition(f, ConditionType.BeginsWith, referenceField.EnteredText)));
                    }

                    if (!DynamicGridViewModel.HasPaging || ignorePages)
                    {
                        var records = DynamicGridViewModel.RecordService.RetreiveAll(query);
                        return new GetGridRecordsResponse(records);
                    }
                    else
                    {
                        return DynamicGridViewModel.GetGridRecordPage(query);
                    }
                };

            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                PageSize = MaxRecordsForLookup,
                GetGridRecords = getGridRecords,
                OnDoubleClick = OnDoubleClick,
                ViewType = ViewType.LookupView,
                RecordService = referenceField.LookupService,
                FormController = new FormController(referenceField.LookupService, null, referenceField.RecordEntryViewModel.ApplicationController),
                RecordType = referenceField.RecordTypeToLookup,
                IsReadOnly = true,
            };
        }

        protected int MaxRecordsForLookup
        {
            get { return 11; }
        }

        private Action<IRecord> OnRecordSelected { get; set; }

        public void OnKeyDown()
        {
        }

        public void OnDoubleClick()
        {
            SetLookupToSelectedRow();
        }

        public void SetLookupToSelectedRow()
        {
            if (DynamicGridViewModel.SelectedRow != null)
                OnRecordSelected(DynamicGridViewModel.SelectedRow.Record);
        }

        public void MoveDown()
        {
            try
            {
                if (DynamicGridViewModel.GridRecords != null && DynamicGridViewModel.GridRecords.Any())
                {
                    var index = -1;
                    if (DynamicGridViewModel.SelectedRow != null)
                        index = DynamicGridViewModel.GridRecords.IndexOf(DynamicGridViewModel.SelectedRow);
                    index++;
                    if (index > DynamicGridViewModel.GridRecords.Count - 1)
                        index = 0;
                    DynamicGridViewModel.SelectedRow = DynamicGridViewModel.GridRecords[index];
                }
            }
            catch
            {
            }
        }

        public void MoveUp()
        {
            try
            {
                if (DynamicGridViewModel.GridRecords != null && DynamicGridViewModel.GridRecords.Any())
                {
                    var index = DynamicGridViewModel.GridRecords.Count;
                    if (DynamicGridViewModel.SelectedRow != null)
                        index = DynamicGridViewModel.GridRecords.IndexOf(DynamicGridViewModel.SelectedRow);
                    index--;
                    if (index < 0)
                        index = DynamicGridViewModel.GridRecords.Count - 1;
                    DynamicGridViewModel.SelectedRow = DynamicGridViewModel.GridRecords[index];
                }
            }
            catch
            {
            }
        }

        public DynamicGridViewModel DynamicGridViewModel { get; set; }
    }
}