using JosephM.Application.ViewModel.Grid;
using JosephM.Core.Attributes;
using JosephM.Record.Metadata;
using JosephM.Record.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class StringAutocompleteViewModel : ViewModelBase
    {
        private bool _displayTypeAhead;

        public StringAutocompleteViewModel(StringFieldViewModel stringField, IEnumerable<string> autocompleteStrings)
            : base(stringField.ApplicationController)
        {
            SearchText = stringField.Value;
            var typeAheads = autocompleteStrings.Select(s => new TypeAheadOption(s)).ToArray();
            var typeAheadOptions = new TypeAheadOptions();
            typeAheadOptions.Options = typeAheads
                .Where(ta => SearchText == null || (ta.Value?.ToLower().StartsWith(SearchText.ToLower()) ?? false));
            var typeAheadRecordService = new ObjectRecordService(typeAheadOptions, ApplicationController);
            var formController = FormController.CreateForObject(typeAheads, ApplicationController, null);

            Func<bool, GetGridRecordsResponse> getGridRecords = (ignorePages) =>
                {
                    var records = typeAheadRecordService
                    .GetLinkedRecords(typeof(TypeAheadOption).AssemblyQualifiedName, typeof(TypeAheadOptions).AssemblyQualifiedName, nameof(TypeAheadOptions.Options), typeAheads.ToString())
                    .Take(MaxRecordsForLookup)
                    .ToArray();
                    if(stringField.DisplayAutocomplete && !records.Any())
                        stringField.DisplayAutocomplete = false;
                    return new GetGridRecordsResponse(records);
                };

            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                FormController = formController,
                GetGridRecords = getGridRecords,
                OnDoubleClick = OnDoubleClick,
                ViewType = ViewType.LookupView,
                RecordService = typeAheadRecordService,
                RecordType = typeof(TypeAheadOption).AssemblyQualifiedName,
                IsReadOnly = true,
                DisplayHeaders= false,
                NoMargins = true
            };
            StringField = stringField;
        }

        protected bool DisplayTypeAhead { get => _displayTypeAhead; set => _displayTypeAhead = value; }

        protected int MaxRecordsForLookup
        {
            get { return 11; }
        }

        public void OnKeyDown()
        {
        }

        public void OnDoubleClick()
        {
            SetToSelectedRow();
        }

        public void SetToSelectedRow()
        {
            if (DynamicGridViewModel.SelectedRow != null)
            {
                StringField.Value = DynamicGridViewModel.SelectedRow.GetStringFieldFieldViewModel(nameof(TypeAheadOption.Value)).Value;
                StringField.DisplayAutocomplete = false;
            }
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
        public StringFieldViewModel StringField { get; }

        public string SearchText { get; set; }

        public class TypeAheadOptions
        {
            public IEnumerable<TypeAheadOption> Options { get; set; }
        }

        public class TypeAheadOption
        {
            public TypeAheadOption(string value)
            {
                Value = value;
            }

            [GridWidth(400)]
            public string Value { get; set; }
        }
    }
}