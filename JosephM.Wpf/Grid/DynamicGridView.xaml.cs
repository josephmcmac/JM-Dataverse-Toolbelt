#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using JosephM.Application.ViewModel.Grid;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using System.Windows.Media;
using System;
using System.ComponentModel;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Wpf.Grid
{
    /// <summary>
    ///     Interaction logic for SubGrid.xaml
    /// </summary>
    public partial class DynamicGridView : UserControl
    {
        public DynamicGridView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            MouseDoubleClick += OnMouseDoubleClick;
            MouseLeftButtonUp += OnMouseClick;
            MouseUp += OnMouseClick;
            KeyDown += OnKeyDown;

            var dp = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
            dp.AddValueChanged(PageDescriptionTextBlock, TextBlock_SourceUpdated);
        }

        private void TextBlock_SourceUpdated(object sender, EventArgs e)
        {
            var border = VisualTreeHelper.GetChild(DataGrid, 0) as Decorator;
            if (border != null)
            {
                var scrollViewer = border.Child as ScrollViewer;
                if (scrollViewer != null)
                    scrollViewer.ScrollToTop();
            }
        }

        private void AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
        }

        private DynamicGridViewModel GridSectionViewModel
        {
            get { return DataContext as DynamicGridViewModel; }
        }

        protected DataGrid DynamicDataGrid { get { return DataGrid; } }

        /// <summary>
        ///     Generates the data grid when the view model is set
        /// </summary>
        public void OnDataContextChanged(object s, DependencyPropertyChangedEventArgs e)
        {
            DynamicDataGrid.AutoGenerateColumns = false;
            CreateDataGrid(GridSectionViewModel, DynamicDataGrid);
            DynamicDataGrid.AddingNewItem += AddingNewItem;
            DynamicDataGrid.LoadingRow += LoadingRow;
        }

        private void LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (DynamicGridViewModel != null)
            {
                if (DynamicGridViewModel.SortCount > 0)
                {
                    DynamicGridViewModel.SortCount--;
                }
                if (DynamicGridViewModel.SortCount == 0)
                {
                    if (SortingLoadGrid.Visibility == Visibility.Visible)
                        SortingLoadGrid.Visibility = Visibility.Collapsed;
                    if (SortingMainGrid.Visibility == Visibility.Hidden)
                        SortingMainGrid.Visibility = Visibility.Visible;
                }
            }
        }

        protected void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null
                && !(columnHeader.Column is DeleteRowColumn)
                && !(columnHeader.Column is EditRowColumn))
            {
                var sortMember = columnHeader.Column.SortMemberPath;
                if (sortMember != null)
                {
                    if (DynamicGridViewModel.GridRecords.Any())
                    {
                        //due to the heavy xaml structure for the view model views
                        //they sometimes take an eternity to sort
                        //especially if lookup, and multiselect fields and heap of rows
                        //so do this hack to display loading while it sorts
                        //the row load event in the view will decrease the sort count until loaded
                        //could not use bindings for this as they were not processing into the ui thread in correct sequence
                        //so have just hacked this way in code behind which seems to take immediate effect
                        SortingLoadGrid.Visibility = Visibility.Visible;
                        SortingMainGrid.Visibility = Visibility.Hidden;
                        DynamicGridViewModel.SortIt(sortMember.Trim(new[] { '[', ']' }));
                    }
                }
            }
        }

        protected DynamicGridViewModel DynamicGridViewModel
        {
            get { return DataContext as DynamicGridViewModel; }
        }

        /// <summary>
        ///     Initialeses DataGrid And Generates Columns From The Bound ViewModels Grid Fields
        /// </summary>
        /// <param name="gridSectionViewModel"></param>
        /// <param name="dynamicDataGrid"></param>
        public static void CreateDataGrid(DynamicGridViewModel gridSectionViewModel, DataGrid dynamicDataGrid)
        {
            //want to do any service calls asynchronously
            dynamicDataGrid.CanUserAddRows = false;
            dynamicDataGrid.Columns.Clear();
            if (gridSectionViewModel != null)
            {
                gridSectionViewModel.ApplicationController.DoOnAsyncThread(() =>
                {
                    if (gridSectionViewModel.FieldMetadata == null)
                        return;
                    var columnMetadata = new List<ColumnMetadata>();
                    foreach (var gridField in gridSectionViewModel.FieldMetadata.OrderBy(gf => gf.Order))
                    {
                        var fieldName = gridField.FieldName;
                        var fieldType = gridSectionViewModel.RecordService.GetFieldType(fieldName,
                            gridSectionViewModel
                                .RecordType);

                        var header = gridSectionViewModel.RecordService.GetFieldLabel(fieldName,
                            gridSectionViewModel.RecordType);
                        var thisColumn = new ColumnMetadata(fieldName, header, fieldType, gridField.WidthPart,
                            gridField.IsEditable);
                        columnMetadata.Add(thisColumn);
                    }

                    gridSectionViewModel.ApplicationController.DoOnMainThread(() =>
                    {
                        if (gridSectionViewModel.CanDelete)
                        {
                            var deleteColumn = new DeleteRowColumn();
                            deleteColumn.Binding = new Binding("DeleteRowViewModel");
                            dynamicDataGrid.Columns.Add(deleteColumn);

                        }
                        if (gridSectionViewModel.CanEdit)
                        {
                            var editColumn = new EditRowColumn();
                            editColumn.Binding = new Binding("EditRowViewModel");
                            dynamicDataGrid.Columns.Add(editColumn);
                        }
                        foreach (var column in columnMetadata)
                        {
                            var cellBinding = new Binding
                            {
                                Path = new PropertyPath(string.Concat("[", column.FieldName, "]")),
                                Mode = BindingMode.TwoWay
                            };
                            DataGridColumn dataGridField;
                            if (column.FieldType == RecordFieldType.Boolean)
                                dataGridField = new GridBooleanColumn
                                {
                                    Binding = cellBinding
                                };
                            else if (column.FieldType == RecordFieldType.RecordType)
                                dataGridField = new GridPicklistColumn()
                                {
                                    Binding = cellBinding
                                };
                            else if (column.FieldType == RecordFieldType.RecordField)
                            {
                                var metadata = gridSectionViewModel.RecordService.GetFieldMetadata(column.FieldName, gridSectionViewModel.RecordType);
                                if (metadata.IsMultiSelect)
                                {
                                    dataGridField = new GridMultiSelectColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                                else
                                {
                                    dataGridField = new GridPicklistColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                            }
                            else if (column.FieldType == RecordFieldType.Picklist)
                            {
                                var metadata = gridSectionViewModel.RecordService.GetFieldMetadata(column.FieldName, gridSectionViewModel.RecordType);
                                if (metadata.IsMultiSelect)
                                {
                                    dataGridField = new GridMultiSelectColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                                else
                                {
                                    dataGridField = new GridPicklistColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                            }
                            else if (column.FieldType == RecordFieldType.Lookup)
                            {
                                if (gridSectionViewModel.FormController.FormService != null
                                    &&
                                    gridSectionViewModel.FormController.FormService.UsePicklist(column.FieldName,
                                        gridSectionViewModel.RecordType))
                                {
                                    dataGridField = new GridLookupPicklistColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                                else
                                {
                                    dataGridField = new GridLookupColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                            }
                            else if (column.FieldType == RecordFieldType.Password)
                            {
                                dataGridField = new GridPasswordColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Enumerable)
                            {
                                dataGridField = new GridEnumerableColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Object)
                            {
                                dataGridField = new GridLookupPicklistColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.FileRef)
                            {
                                dataGridField = new GridFileRefColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Integer)
                            {
                                var format = gridSectionViewModel.RecordService.GetIntegerFormat(column.FieldName,
                                        gridSectionViewModel.RecordType);
                                if(format == IntegerType.TimeZone || format == IntegerType.Language)
                                {
                                    dataGridField = new GridIntPicklistColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                                else
                                {
                                    dataGridField = new GridIntColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                            }
                            else if (column.FieldType == RecordFieldType.BigInt)
                            {
                                dataGridField = new GridIntColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Date)
                            {
                                dataGridField = new GridDateColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Decimal)
                            {
                                dataGridField = new GridDecimalColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Double)
                            {
                                dataGridField = new GridDoubleColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Money)
                            {
                                dataGridField = new GridMoneyColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Url)
                            {
                                dataGridField = new GridUrlColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.ActivityParty)
                            {
                                dataGridField = new GridActivityPartyColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else
                            {
                                dataGridField = new GridStringColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            dataGridField.Header = column.FieldLabel;
                            dataGridField.Width = new DataGridLength(column.WidthPart,
                                DataGridLengthUnitType.Pixel);
                            var isFormReadonly = gridSectionViewModel.IsReadOnly;
                            var isWriteable = gridSectionViewModel?.RecordService?.GetFieldMetadata(column.FieldName, gridSectionViewModel.RecordType).Createable == true
                                || gridSectionViewModel?.RecordService?.GetFieldMetadata(column.FieldName, gridSectionViewModel.RecordType).Writeable == true;
                            dataGridField.IsReadOnly = isFormReadonly || !isWriteable;
                            var description = gridSectionViewModel?.RecordService?.GetFieldMetadata(column.FieldName, gridSectionViewModel.RecordType).Description;
                            //todo this removes the standard xaml setters including the click to sort
                            //var style = new Style(typeof(DataGridColumnHeader));
                            //style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, description));
                            //dataGridField.HeaderStyle = style;
                            dynamicDataGrid.Columns.Add(dataGridField);
                        }
                        var dataGridBinding = new Binding
                        {
                            Path = new PropertyPath("GridRecords"),
                            Mode = BindingMode.TwoWay
                        };
                        dynamicDataGrid.SetBinding(ItemsControl.ItemsSourceProperty, dataGridBinding);
                        var selectedItemBinding = new Binding
                        {
                            Path = new PropertyPath("SelectedRow"),
                            Mode = BindingMode.TwoWay
                        };
                        dynamicDataGrid.SetBinding(Selector.SelectedItemProperty, selectedItemBinding);
                    });
                });
            }
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        public void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GridSectionViewModel.OnDoubleClick();
        }

        public void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            //GridSectionViewModel.OnClick();
        }

        private class ColumnMetadata
        {
            public ColumnMetadata(string fieldName, string fieldLabel, RecordFieldType fieldType, double widthPart,
                bool isEditable)
            {
                FieldName = fieldName;
                FieldLabel = fieldLabel;
                FieldType = fieldType;
                WidthPart = widthPart;
                IsEditable = isEditable;
            }

            public string FieldName { get; private set; }
            public string FieldLabel { get; private set; }
            public RecordFieldType FieldType { get; private set; }
            public double WidthPart { get; private set; }
            public bool IsEditable { get; private set; }
        }

        private object _lockObject = new object();
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lock (_lockObject)
            {
                try
                {
                    if (GridSectionViewModel != null)
                    {
                        if (e != null)
                        {
                            if (e.AddedItems != null)
                            {
                                foreach (var item in e.AddedItems)
                                {
                                    if (item is GridRowViewModel)
                                        ((GridRowViewModel)item).IsSelected = true;
                                }
                            }
                            if (e.RemovedItems != null)
                            {
                                foreach (var item in e.RemovedItems)
                                {
                                    if (item is GridRowViewModel)
                                        ((GridRowViewModel)item).IsSelected = false;
                                }
                            }
                        }
                        GridSectionViewModel.OnSelectionsChanged();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.DisplayString());
                }
            }
        }
    }
}