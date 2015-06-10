#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Record.Application.RecordEntry.Field;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class FieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BooleanFieldTemplate { get; set; }
        public DataTemplate ComboBoxFieldTemplate { get; set; }
        public DataTemplate DateFieldTemplate { get; set; }
        public DataTemplate StringFieldTemplate { get; set; }
        public DataTemplate IntegerFieldTemplate { get; set; }
        public DataTemplate ExcelFileFieldTemplate { get; set; }
        public DataTemplate LookupFieldTemplate { get; set; }
        public DataTemplate PasswordFieldTemplate { get; set; }
        public DataTemplate FolderFieldTemplate { get; set; }
        public DataTemplate StringEnumerableFieldTemplate { get; set; }
        public DataTemplate RecordTypeFieldTemplate { get; set; }
        public DataTemplate RecordFieldFieldTemplate { get; set; }
        public DataTemplate FileRefFieldTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is BooleanFieldViewModel)
                return BooleanFieldTemplate;
            if (item is PicklistFieldViewModel)
                return ComboBoxFieldTemplate;
            if (item is DateFieldViewModel)
                return DateFieldTemplate;
            if (item is StringFieldViewModel)
                return StringFieldTemplate;
            if (item is IntegerFieldViewModel)
                return IntegerFieldTemplate;
            if (item is ExcelFileFieldViewModel)
                return ExcelFileFieldTemplate;
            if (item is LookupFieldViewModel)
                return LookupFieldTemplate;
            if (item is PasswordFieldViewModel)
                return PasswordFieldTemplate;
            if (item is FolderFieldViewModel)
                return FolderFieldTemplate;
            if (item is StringEnumerableFieldViewModel)
                return StringEnumerableFieldTemplate;
            if (item is RecordTypeFieldViewModel)
                return RecordTypeFieldTemplate;
            if (item is RecordFieldFieldViewModel)
                return RecordFieldFieldTemplate;
            if (item is ObjectFieldViewModel)
                return LookupFieldTemplate;
            if (item is FileRefFieldViewModel)
                return FileRefFieldTemplate;

            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}