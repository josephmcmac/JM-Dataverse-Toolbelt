#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using JosephM.Record.Application.Grid;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Wpf.Grid
{
    public abstract class DynamicGridView : UserControl
    {
        protected DynamicGridView()
        {
            DataContextChanged += OnDataContextChanged;
            MouseDoubleClick += OnMouseDoubleClick;
            MouseDown += OnMouseClick;
            KeyDown += OnKeyDown;
        }

        private void AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
        }

        private IDynamicGridViewModel GridSectionViewModel
        {
            get { return DataContext as IDynamicGridViewModel; }
        }

        protected abstract DataGrid DynamicDataGrid { get; }

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
        }

        /// <summary>
        ///     Initialeses DataGrid And Generates Columns From The Bound ViewModels Grid Fields
        /// </summary>
        /// <param name="gridSectionViewModel"></param>
        /// <param name="dynamicDataGrid"></param>
        public static void CreateDataGrid(IDynamicGridViewModel gridSectionViewModel, DataGrid dynamicDataGrid)
        {
            //want to do any service calls asynchronously
            dynamicDataGrid.CanUserAddRows = false;
            dynamicDataGrid.Columns.Clear();
            if (gridSectionViewModel != null)
            {
                gridSectionViewModel.DoOnAsynchThread(() =>
                {
                    gridSectionViewModel.DoWhileLoading("Loading Lookup", () =>
                    {
                        var columnMetadata = new List<ColumnMetadata>();
                        foreach (var gridField in gridSectionViewModel.RecordFields.OrderBy(gf => gf.Order))
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

                        gridSectionViewModel.DoOnMainThread(() =>
                        {
                            if (gridSectionViewModel.DynamicGridViewModelItems.CanDelete)
                            {
                                var deleteColumn = new DeleteRowColumn();
                                deleteColumn.Binding = new Binding("DeleteRowViewModel");
                                dynamicDataGrid.Columns.Add(deleteColumn);
                            }
                            if (gridSectionViewModel.DynamicGridViewModelItems.CanEdit)
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
                                    dataGridField = new GridPicklistColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                else if (column.FieldType == RecordFieldType.Picklist)
                                    dataGridField = new GridPicklistColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                else if (column.FieldType == RecordFieldType.Lookup)
                                {
                                    dataGridField = new GridLookupColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                                else if (column.FieldType == RecordFieldType.Password)
                                {
                                    dataGridField = new GridPasswordColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                }
                                else
                                    dataGridField = new GridStringColumn()
                                    {
                                        Binding = cellBinding
                                    };
                                dataGridField.Header = column.FieldLabel;
                                dataGridField.IsReadOnly = !column.IsEditable;
                                dataGridField.Width = new DataGridLength(column.WidthPart,
                                    DataGridLengthUnitType.Pixel);
                                dataGridField.IsReadOnly = gridSectionViewModel.IsReadOnly;
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
                });
            }
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        public void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GridSectionViewModel.DynamicGridViewModelItems.OnDoubleClick();
        }

        public void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
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
    }
}