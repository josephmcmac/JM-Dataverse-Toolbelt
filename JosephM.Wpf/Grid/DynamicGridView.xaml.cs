using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using JosephM.Record.Extentions;
using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
            if (DynamicGridViewModel == null || !DynamicGridViewModel.HasNavigated)
                return;
            //the point of this is to scroll to the top of the grid
            //when the user navigates to a different page
            if (VisualTreeHelper.GetChildrenCount(DataGrid) > 0)
            {
                //this first part is for the query view model context
                //basically if navigate in a query result we want the query view to scroll to the top
                var scroller = FindParentOfType<ScrollViewer>(this);
                if (scroller != null)// && scroller.Name == "QueryScroll")
                {
                    var transform = TransformToVisual(scroller) as MatrixTransform;
                    if (transform != null)
                        scroller.ScrollToVerticalOffset(scroller.VerticalOffset + transform.Matrix.OffsetY);
                }

                //this i think is the scroller in the datagrid itself if it has a limited height
                //in that case we want it to scroll to the top
                //    var border = VisualTreeHelper.GetChild(DataGrid, 0) as Decorator;
                //if (border != null)
                //{
                //    var scrollViewer = border.Child as ScrollViewer;
                //    if (scrollViewer != null)
                //        scrollViewer.ScrollToTop();
                //}
            }
        }

        public static T FindParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentDepObj = child;
            do
            {
                parentDepObj = VisualTreeHelper.GetParent(parentDepObj);
                T parent = parentDepObj as T;
                if (parent != null) return parent;
            }
            while (parentDepObj != null);
            return null;
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
            //okay so this method is because the asynch nature of the grid means it takes a while to for the ui 
            //to reload the sorted grid
            //so display loading while counting the number of rows added during loading until they are all added
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
                if (DynamicGridViewModel.SortCount > 0)
                {
                    //uggh so this seems to have stopped working if the grid exceeds the height of the monitor
                    //think it is only loading the rows in frame
                    //so let do a timeout for a second and if it is still loading with the same counter then display
                    new SortSpawn(DynamicGridViewModel.SortCount, DynamicGridViewModel, SortingMainGrid, SortingLoadGrid).DoIt();
                }
            }
        }

        public class SortSpawn
        {
            public SortSpawn(int currentCount, DynamicGridViewModel grid, System.Windows.Controls.Grid sortingMainGrid, System.Windows.Controls.Grid sortingLoadGrid)
            {
                CurrentCount = currentCount;
                Grid = grid;
                SortingMainGrid = sortingMainGrid;
                SortingLoadGrid = sortingLoadGrid;
            }

            public int CurrentCount { get; }
            public DynamicGridViewModel Grid { get; }
            public System.Windows.Controls.Grid SortingMainGrid { get; }
            public System.Windows.Controls.Grid SortingLoadGrid { get; }

            public void DoIt()
            {
                Grid.ApplicationController.DoOnAsyncThread(() =>
                {
                    Thread.Sleep(1000);
                    if(Grid.SortCount == CurrentCount)
                    {
                        Grid.ApplicationController.DoOnMainThread(() =>
                        {
                            if (SortingLoadGrid.Visibility == Visibility.Visible)
                                SortingLoadGrid.Visibility = Visibility.Collapsed;
                            if (SortingMainGrid.Visibility == Visibility.Hidden)
                                SortingMainGrid.Visibility = Visibility.Visible;
                        });
                    }
                });
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
                dynamicDataGrid.HeadersVisibility = gridSectionViewModel.DisplayHeaders ? DataGridHeadersVisibility.Column : DataGridHeadersVisibility.None;
                gridSectionViewModel.ApplicationController.DoOnAsyncThread(() =>
                {
                    if (gridSectionViewModel.FieldMetadata == null)
                        return;
                    var columnMetadata = new List<ColumnMetadata>();
                    foreach (var gridField in gridSectionViewModel.FieldMetadata.OrderBy(gf => gf.Order))
                    {
                        var thisRecordType = gridField.AltRecordType ?? gridSectionViewModel.RecordType;
                        var fieldName = gridField.FieldName;
                        if (gridSectionViewModel.RecordService.FieldExists(fieldName, thisRecordType))
                        {
                            var fieldMetadata = gridSectionViewModel.RecordService.GetFieldMetadata(fieldName, thisRecordType);
                            var thisColumn = new ColumnMetadata(thisRecordType, fieldName, gridField.OverrideLabel ?? fieldMetadata.DisplayName ?? fieldName, fieldMetadata.FieldType, gridField.WidthPart,
                                gridField.IsReadOnly, fieldMetadata.Description, gridSectionViewModel.GetHorizontalJustify(fieldMetadata.FieldType), gridSectionViewModel.DisplayHeaders, gridField.AliasedFieldName);
                            columnMetadata.Add(thisColumn);
                        }
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
                                Path = new PropertyPath(string.Concat("[", column.AliasedFieldName ?? column.FieldName, "]")),
                                Mode = BindingMode.TwoWay
                            };
                            DataGridColumn dataGridField;
                            if (column.FieldType == RecordFieldType.Url)
                            {
                                dataGridField = new GridUrlColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (gridSectionViewModel.IsReadOnly)
                            {
                                dataGridField = new GridStringDisplayOnlyColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Boolean
                            || column.FieldType == RecordFieldType.ManagedProperty)
                            {
                                dataGridField = new GridBooleanColumn
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.RecordType)
                            {
                                if (gridSectionViewModel.FormController.FormService != null
                                    && gridSectionViewModel.FormController.FormService.UsePicklist(column.FieldName, column.RecordType))
                                {
                                    dataGridField = new GridRecordTypePicklistColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                                else
                                {
                                    dataGridField = new GridRecordTypeColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                            }
                            else if (column.FieldType == RecordFieldType.RecordField)
                            {
                                var metadata = gridSectionViewModel.RecordService.GetFieldMetadata(column.FieldName, column.RecordType);
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
                                var metadata = gridSectionViewModel.RecordService.GetFieldMetadata(column.FieldName, column.RecordType);
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
                                        column.RecordType))
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
                                        column.RecordType);
                                if (format == IntegerType.TimeZone || format == IntegerType.Language)
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
                            else if (column.FieldType == RecordFieldType.ActivityParty)
                            {
                                dataGridField = new GridActivityPartyColumn()
                                {
                                    Binding = cellBinding
                                };
                            }
                            else if (column.FieldType == RecordFieldType.Uniqueidentifier)
                            {
                                dataGridField = new GridUniqueIdentifierColumn()
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
                            dataGridField.Header = column;
                            dataGridField.Width = new DataGridLength(column.WidthPart,
                                DataGridLengthUnitType.Pixel);
                            var isFormReadonly = gridSectionViewModel.IsReadOnly;
                            var isWriteable = gridSectionViewModel?.RecordService?.GetFieldMetadata(column.FieldName, column.RecordType).Createable == true
                                || gridSectionViewModel?.RecordService?.GetFieldMetadata(column.FieldName, column.RecordType).Writeable == true;
                            dataGridField.IsReadOnly = isFormReadonly || column.IsReadOnly || !isWriteable;
                            var description = gridSectionViewModel?.RecordService?.GetFieldMetadata(column.FieldName, column.RecordType).Description;
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

        public class ColumnMetadata
        {
            public ColumnMetadata(string recordType, string fieldName, string fieldLabel, RecordFieldType fieldType, double widthPart,
                bool isReadOnly, string tooltip, HorizontalJustify justify, bool displayColumnHeader, string aliasedFieldName)
            {
                RecordType = recordType;
                AliasedFieldName = aliasedFieldName;
                DisplayColumnHeader = displayColumnHeader;
                FieldName = fieldName;
                FieldLabel = fieldLabel;
                FieldType = fieldType;
                WidthPart = widthPart;
                IsReadOnly = isReadOnly;
                Tooltip = tooltip;
                HorizontalJustify = justify;
            }

            public bool IsSortable
            {
                get { return FieldLabel != null; }
            }

            public bool DisplayColumnHeader { get; set; }

            public string RecordType { get; private set; }
            public string AliasedFieldName { get; private set; }
            public string FieldName { get; private set; }
            public string FieldLabel { get; private set; }
            public RecordFieldType FieldType { get; private set; }
            public double WidthPart { get; private set; }
            public bool IsReadOnly { get; private set; }
            public string Tooltip { get; }
            public HorizontalJustify HorizontalJustify { get; private set; }
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

        private void DataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            var fieldNames = new Dictionary<string, string>();
            var columns = new List<DataGridColumn>();

            foreach (var column in DynamicDataGrid.Columns)
            {
                if (column.Header is ColumnMetadata columnMetadata)
                {
                    if (!fieldNames.ContainsKey(columnMetadata.AliasedFieldName ?? columnMetadata.FieldName))
                    {
                        columns.Add(column);
                        fieldNames.Add(columnMetadata.AliasedFieldName ?? columnMetadata.FieldName, columnMetadata.FieldLabel);
                    }
                }
            }

            if (fieldNames.Any())
            {
                if (e.IsColumnHeadersRow)
                {
                    e.ClipboardRowContent.Clear();
                    var i = 0;
                    foreach (var label in fieldNames.Values)
                    {
                        e.ClipboardRowContent.Add(new DataGridClipboardCellContent(e.Item, columns[i++], label));
                    }
                }
                else
                {
                    if(e.Item is GridRowViewModel rowViewModel)
                    {
                        e.ClipboardRowContent.Clear();
                        var i = 0;
                        foreach (var field in fieldNames.Keys)
                        {
                            e.ClipboardRowContent.Add(new DataGridClipboardCellContent(e.Item, columns[i++], rowViewModel.GetFieldViewModel(field).StringDisplay));
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DynamicGridViewModel.NextPageButton.Command.Execute();
        }
    }
}