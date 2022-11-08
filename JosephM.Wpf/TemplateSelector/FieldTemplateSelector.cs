#region

using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class FieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BooleanFieldTemplate { get; set; }
        public DataTemplate BooleanPicklistFieldTemplate { get; set; }
        public DataTemplate ComboBoxFieldTemplate { get; set; }
        public DataTemplate DateFieldTemplate { get; set; }
        public DataTemplate StringFieldTemplate { get; set; }
        public DataTemplate StringFieldWithAutocompleteTemplate { get; set; }
        public DataTemplate StringFieldDisplayOnlyTemplate { get; set; }
        public DataTemplate IntegerFieldTemplate { get; set; }
        public DataTemplate IntegerPicklistFieldTemplate { get; set; }
        public DataTemplate LookupFieldTemplate { get; set; }
        public DataTemplate LookupFieldPicklistTemplate { get; set; }
        public DataTemplate PasswordFieldTemplate { get; set; }
        public DataTemplate FolderFieldTemplate { get; set; }
        public DataTemplate StringEnumerableFieldTemplate { get; set; }
        public DataTemplate RecordTypeFieldTemplate { get; set; }
        public DataTemplate RecordTypePicklistFieldTemplate { get; set; }
        public DataTemplate RecordFieldFieldTemplate { get; set; }
        public DataTemplate FileRefFieldTemplate { get; set; }
        public DataTemplate EnumerableFieldTemplate { get; set; }
        public DataTemplate EnumerableFieldTemplateUniform { get; set; }
        public DataTemplate DecimalFieldTemplate { get; set; }
        public DataTemplate UrlFieldTemplate { get; set; }
        public DataTemplate MultiSelectFieldTemplate { get; set; }
        public DataTemplate ActivityPartyFieldTemplate { get; set; }
        public DataTemplate UniqueIdentifierFieldTemplate { get; set; }
        public DataTemplate UnmatchedFieldTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is EnumerableFieldViewModel enumerableVm)
            {
                if (enumerableVm.IsGridOnlyEntryField)
                {
                    return EnumerableFieldTemplateUniform;
                }
                else
                {
                    return EnumerableFieldTemplate;
                }
            }
            if (item is FieldViewModelBase fvmb
                && fvmb.FixedReadOnly)
            {
                return StringFieldDisplayOnlyTemplate;
            }
            if (item is StringFieldViewModel sfvm)
            {
                if (sfvm.AutocompleteFunction != null)
                {
                    return StringFieldWithAutocompleteTemplate;
                }
                else
                {
                    return StringFieldTemplate;
                }
            }
            if (item is BooleanFieldViewModel bfvm)
            {
                if (bfvm.UsePicklist)
                {
                    return BooleanPicklistFieldTemplate;
                }
                else
                {
                    return BooleanFieldTemplate;
                }
            }
            if (item is PicklistFieldViewModel)
            {
                return ComboBoxFieldTemplate;
            }
            if (item is DateFieldViewModel)
            {
                return DateFieldTemplate;
            }
            if (item is IntegerFieldViewModel ifvm)
            {
                if (ifvm.UsePicklist)
                {
                    return IntegerPicklistFieldTemplate;
                }
                else
                {
                    return IntegerFieldTemplate;
                }
            }
            if (item is BigIntFieldViewModel)
            {
                return IntegerFieldTemplate;
            }
            if (item is LookupFieldViewModel)
            {
                if (((LookupFieldViewModel)item).UsePicklist)
                {
                    return LookupFieldPicklistTemplate;
                }
                else
                {
                    return LookupFieldTemplate;
                }
            }
            if (item is PasswordFieldViewModel)
            {
                return PasswordFieldTemplate;
            }
            if (item is FolderFieldViewModel)
            {
                return FolderFieldTemplate;
            }
            if (item is StringEnumerableFieldViewModel)
            {
                return StringEnumerableFieldTemplate;
            }
            if (item is RecordTypeFieldViewModel rtfvm)
            {
                if (rtfvm.UsePicklist)
                {
                    return RecordTypePicklistFieldTemplate;
                }
                else
                {
                    return RecordTypeFieldTemplate;
                }
            }
            if (item is RecordFieldFieldViewModel)
            {
                return RecordFieldFieldTemplate;
            }
            if (item is ObjectFieldViewModel)
            {
                if (((ObjectFieldViewModel)item).UsePicklist)
                {
                    return LookupFieldPicklistTemplate;
                }
                else
                {
                    return LookupFieldTemplate;
                }
            }
            if (item is FileRefFieldViewModel)
            {
                return FileRefFieldTemplate;
            }
            if (item is DecimalFieldViewModel)
            {
                return DecimalFieldTemplate;
            }
            if (item is DoubleFieldViewModel)
            {
                return DecimalFieldTemplate;
            }
            if (item is MoneyFieldViewModel)
            {
                return DecimalFieldTemplate;
            }
            if (item is UrlFieldViewModel)
            {
                return UrlFieldTemplate;
            }
            if (item is IMultiSelectFieldViewModel)
            {
                return MultiSelectFieldTemplate;
            }
            if (item is ActivityPartyFieldViewModel)
            {
                return ActivityPartyFieldTemplate;
            }
            if (item is UniqueIdentifierFieldViewModel)
            {
                return UniqueIdentifierFieldTemplate;
            }
            else
            {
                return UnmatchedFieldTemplate;
            }
        }
    }
}