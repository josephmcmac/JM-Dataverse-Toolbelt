using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JosephM.Application.ViewModel.Query
{
    public class ColumnEditDialogViewModel : TabAreaViewModelBase
    {
        private string RecordType { get; set; }
        public Action<IEnumerable<SelectableColumn>> ApplySelections { get; private set; }
        private IRecordService RecordService { get; set; }

        public ColumnEditDialogViewModel(string recordType, IEnumerable<KeyValuePair<string, double>> currentColumns, IRecordService recordService, Action<IEnumerable<SelectableColumn>> applySelections, Action onCancel, IApplicationController applicationController)
            : base(applicationController)
        {
            RecordService = recordService;
            RecordType = recordType;
            ApplySelections = applySelections;
            var columnsCurrenltySelected =
                currentColumns.Select(si => new SelectableColumn(si.Key, RecordService.GetFieldLabel(si.Key, RecordType), si.Value, RemoveCurrentField, AddCurrentField, ApplicationController))
                .ToArray();
            CurrentColumns = new ObservableCollection<SelectableColumn>(columnsCurrenltySelected);
            var selectableFields = RecordService
                .GetFields(RecordType)
                .Where(f => !CurrentColumns.Any(c => c.FieldName == f))
                .Select(f => new SelectableColumn(f, RecordService.GetFieldLabel(f, RecordType), 200, RemoveCurrentField, AddCurrentField, ApplicationController))
                .OrderBy(sc => sc.FieldLabel)
                .ToArray();
            SelectableColumns = new ObservableCollection<SelectableColumn>(selectableFields);

            ApplyButtonViewModel = new XrmButtonViewModel("Apply Changes", ApplyChanges, ApplicationController, "Apply The Selection Changes");
            CancelButtonViewModel = new XrmButtonViewModel("Cancel Changes", onCancel, ApplicationController, "Cancel The Selection Changes And Return");

            RefreshIsFirstColumn();
        }

        public void AddCurrentItem(SelectableColumn draggedItem, SelectableColumn target = null, bool isAfter = true)
        {
            DoOnMainThread(() =>
            {
                SelectableColumns.Remove(draggedItem);
                CurrentColumns.Remove(draggedItem);
                if (target != null && CurrentColumns.Contains(target))
                    CurrentColumns.Insert(CurrentColumns.IndexOf(target) + (isAfter ? 1 : 0), draggedItem);
                else
                    CurrentColumns.Add(draggedItem);
                RefreshIsFirstColumn();
            });
        }

        public void RefreshIsFirstColumn()
        {
            if(CurrentColumns != null)
            {
                foreach(var column in CurrentColumns.ToArray().Skip(1))
                {
                    if (column.IsFirstColumn)
                        column.IsFirstColumn = false;
                }
                if(CurrentColumns.Any())
                {
                    CurrentColumns.First().IsFirstColumn = true;
                }
            }
        }

        public void AddCurrentField(string fieldName)
        {
            var selectedResults = SelectableColumns.Where(c => c.FieldName == fieldName).ToArray();
            foreach (var result in selectedResults)
            {
                AddCurrentItem(result);
            }
        }

        public void RemoveCurrentField(string fieldName)
        {
            if (CurrentColumns.Count == 1 && CurrentColumns.First().FieldName == fieldName)
            {
                ApplicationController.UserMessage("The Must Be At Least One View Column");
            }
            else
            {
                var selectedResults = CurrentColumns.Where(c => c.FieldName == fieldName).ToArray();
                foreach (var result in selectedResults)
                {
                    CurrentColumns.Remove(result);
                    foreach (var item in SelectableColumns)
                    {
                        if (item.FieldLabel != null && item.FieldLabel.CompareTo(result.FieldLabel) > 0)
                        {
                            SelectableColumns.Insert(SelectableColumns.IndexOf(item), result);
                            break;
                        }
                    }
                    if (!SelectableColumns.Contains(result))
                        SelectableColumns.Add(result);
                }
            }
        }

        public ObservableCollection<SelectableColumn>  CurrentColumns { get; set; }

        public ObservableCollection<SelectableColumn>  SelectableColumns { get; set; }
        public XrmButtonViewModel ApplyButtonViewModel { get; private set; }
        public XrmButtonViewModel CancelButtonViewModel { get; private set; }
        public void ApplyChanges()
        {
            ApplySelections(CurrentColumns);
        }

        public class SelectableColumn : ViewModelBase
        {
            public SelectableColumn(string fieldName, string fieldLabel, double width, Action<string> removeField, Action<string> addField, IApplicationController controller)
                : base(controller)
            {
                FieldName = fieldName;
                FieldLabel = fieldLabel;
                Width = width;
                RemoveCommand = new MyCommand(() => removeField(FieldName));
                AddCommand = new MyCommand(() => addField(FieldName));
            }

            public string FieldName { get; set; }
            public string FieldLabel { get; set; }
            public double Width { get; set; }
            public MyCommand RemoveCommand { get; set; }

            public MyCommand AddCommand { get; set; }

            private bool _isFirstColumn;
            public bool IsFirstColumn
            {
                get
                {
                    return _isFirstColumn;
                }
                set
                {
                    _isFirstColumn = value;
                    OnPropertyChanged(nameof(IsFirstColumn));
                }
            }

        }
    }
}