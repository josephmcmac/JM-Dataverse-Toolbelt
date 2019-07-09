using JosephM.Application.Application;
using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.TabArea;
using JosephM.Core.FieldType;
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
        private PicklistOption _selectedLink;

        private string RecordType { get; set; }
        public Action<IEnumerable<SelectableColumn>> ApplySelections { get; private set; }
        private IRecordService RecordService { get; set; }

        public ColumnEditDialogViewModel(string recordType, IEnumerable<GridFieldMetadata> currentColumns, IRecordService recordService, Action<IEnumerable<SelectableColumn>> applySelections, Action onCancel, IApplicationController applicationController)
            : base(applicationController)
        {
            RecordService = recordService;
            RecordType = recordType;
            ApplySelections = applySelections;

            var currentColumnsSelectables = new List<SelectableColumn>();
            foreach(GridFieldMetadata column in currentColumns)
            {
                string fieldNameIncludingPrefix = column.AliasedFieldName ?? column.FieldName;
                if (fieldNameIncludingPrefix.Contains("."))
                {
                    //aliased columns are stored within this vm in form
                    //lookup|recordtype.fieldname
                    //but externally in form
                    //lookup_recordtype.fieldname
                    //due to | not being a valid alias value
                    fieldNameIncludingPrefix = fieldNameIncludingPrefix.Replace("_" + column.AltRecordType + ".", "|" + column.AltRecordType + ".");
                }
                currentColumnsSelectables.Add(new SelectableColumn(fieldNameIncludingPrefix, column.OverrideLabel ?? RecordService.GetFieldLabel(column.FieldName, RecordType), column.WidthPart, RemoveCurrentField, AddCurrentField, ApplicationController));
            }
            CurrentColumns = new ObservableCollection<SelectableColumn>(currentColumnsSelectables);

            LinkOptions = GetLinkOptionsList(RecordType);
            _selectedLink = LinkOptions.First();

            SelectableColumns = new ObservableCollection<SelectableColumn>(GetSelectableColumnsFor(RecordType));

            ApplyButtonViewModel = new XrmButtonViewModel("Apply Changes", ApplyChanges, ApplicationController, "Apply The Selection Changes");
            CancelButtonViewModel = new XrmButtonViewModel("Cancel Changes", onCancel, ApplicationController, "Cancel The Selection Changes And Return");

            RefreshIsFirstColumn();
        }

        private IEnumerable<SelectableColumn> GetSelectableColumnsFor(string thisType)
        {
            var keyPrefix = "";
            var labelPrefix = "";
            if (SelectedLink != null && SelectedLink.Key.Contains("|"))
            {
                keyPrefix = SelectedLink.Key + ".";
                labelPrefix = SelectedLink.Value + " > ";
            }
            return RecordService
                .GetFields(thisType)
                .Select(f => new SelectableColumn(keyPrefix + f, labelPrefix + RecordService.GetFieldLabel(f, thisType), 200, RemoveCurrentField, AddCurrentField, ApplicationController))
                .Where(sc => !CurrentColumns.Any(c => c.FieldName == sc.FieldName))
                .OrderBy(sc => sc.FieldLabel)
                .ToArray();
        }

        private IEnumerable<PicklistOption> GetLinkOptionsList(string thisType)
        {
            //allow selection of lookup fields to allow inclusion of fields in related entities
            //or the primary entity being queried
            var thisTypeSelection = new PicklistOption(thisType, RecordService.GetDisplayName(thisType));
            var linkOptions = new List<PicklistOption>();
            var lookupFields = RecordService
                .GetFields(thisType)
                .Where(f => RecordService.IsLookup(f, thisType));
            foreach (var field in lookupFields)
            {
                var targetTypes = RecordService.GetLookupTargetType(field, thisType);
                if (targetTypes != null)
                {
                    var fieldLabel = RecordService.GetFieldLabel(field, thisType);
                    var split = targetTypes.Split(',');
                    var areMultipleTypes = split.Count() > 1;
                    foreach (var type in split)
                    {
                        var key = field + "|" + type;
                        var label = fieldLabel + (areMultipleTypes ? $" ({RecordService.GetDisplayName(type)})" : "");
                        linkOptions.Add(new PicklistOption(key, label));
                    }
                }
            }
            linkOptions.Sort();
            linkOptions.Insert(0, thisTypeSelection);
            return linkOptions;
        }

        public IEnumerable<PicklistOption> LinkOptions { get; set; }

        public PicklistOption SelectedLink
        {
            get { return _selectedLink; }
            set
            {
                _selectedLink = value;
                if (value != null)
                {
                    LoadingViewModel.IsLoading = true;
                    //when selected spawn process to change the selectable fields
                    //to those in the selections type
                    ApplicationController.DoOnMainThread(() =>
                    {
                        try
                        {
                            var split = value.Key.Split('|');
                            var targetType = split.Count() > 1
                                ? split[1]
                                : RecordType;
                            SelectableColumns.Clear();
                            var newColumns = GetSelectableColumnsFor(targetType);
                            foreach (var column in newColumns)
                            {
                                SelectableColumns.Add(column);
                            }
                        }
                        catch (Exception ex)
                        {
                            ApplicationController.ThrowException(ex);
                        }
                        finally
                        {
                            LoadingViewModel.IsLoading = false;
                        }
                    });
                }
            }
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
            if (CurrentColumns != null)
            {
                foreach (var column in CurrentColumns.ToArray().Skip(1))
                {
                    if (column.IsFirstColumn)
                        column.IsFirstColumn = false;
                }
                if (CurrentColumns.Any())
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
                ApplicationController.UserMessage("There Must Be At Least One View Column");
            }
            else
            {
                //if removed field active for the current selected link then add it as an option
                if ((!fieldName.Contains(".") && SelectedLink.Key == RecordType)
                    || (fieldName.Contains(".") && fieldName.Split('.')[0] == SelectedLink.Key))
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
        }

        public ObservableCollection<SelectableColumn> CurrentColumns { get; set; }

        public ObservableCollection<SelectableColumn> SelectableColumns { get; set; }
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