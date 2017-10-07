﻿#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace JosephM.Wpf.Grid
{
    internal class GridMoneyColumn : DataGridBoundColumn
    {
        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var field = new GridMoneyFieldView();
            field.SetBinding(FrameworkElement.DataContextProperty, Binding);
            return field;
        }

        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            return null;
        }

        protected override bool CommitCellEdit(FrameworkElement editingElement)
        {
            return true;
        }
    }
}