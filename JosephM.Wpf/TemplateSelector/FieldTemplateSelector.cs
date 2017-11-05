#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Record.Service;

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
        public DataTemplate LookupFieldTemplate { get; set; }
        public DataTemplate LookupFieldPicklistTemplate { get; set; }
        public DataTemplate PasswordFieldTemplate { get; set; }
        public DataTemplate FolderFieldTemplate { get; set; }
        public DataTemplate StringEnumerableFieldTemplate { get; set; }
        public DataTemplate RecordTypeFieldTemplate { get; set; }
        public DataTemplate RecordFieldFieldTemplate { get; set; }
        public DataTemplate FileRefFieldTemplate { get; set; }
        public DataTemplate EnumerableFieldTemplate { get; set; }
        public DataTemplate DecimalFieldTemplate { get; set; }
        public DataTemplate UrlFieldTemplate { get; set; }
        public DataTemplate MultiSelectFieldTemplate { get; set; }

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
            if (item is IntegerFieldViewModel || item is BigIntFieldViewModel)
                return IntegerFieldTemplate;
            if (item is LookupFieldViewModel)
            {
                if (((LookupFieldViewModel) item).UsePicklist)
                    return LookupFieldPicklistTemplate;
                else
                    return LookupFieldTemplate;
            }
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
            {
                if (((ObjectFieldViewModel)item).UsePicklist)
                    return LookupFieldPicklistTemplate;
                else
                    return LookupFieldTemplate;
            }
            if (item is FileRefFieldViewModel)
                return FileRefFieldTemplate;
            if (item is EnumerableFieldViewModel)
                return EnumerableFieldTemplate;
            if (item is DecimalFieldViewModel)
                return DecimalFieldTemplate;
            if (item is DoubleFieldViewModel)
                return DecimalFieldTemplate;
            if (item is MoneyFieldViewModel)
                return DecimalFieldTemplate;
            if (item is UrlFieldViewModel)
                return UrlFieldTemplate;
            if (item is PicklistMultiSelectFieldViewModel)
                return MultiSelectFieldTemplate;
            else
                return StringFieldTemplate;
            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}