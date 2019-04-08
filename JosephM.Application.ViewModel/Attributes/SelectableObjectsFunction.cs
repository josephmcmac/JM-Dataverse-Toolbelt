using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false)]
    public class SelectableObjectsFunction : BulkAddMultiSelectFunction
    {
        public override Type TargetPropertyType
        {
            get { return typeof(ISelectable); }
        }

        public override IEnumerable<PicklistOption> GetSelectionOptions(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var options = new List<PicklistOption>();
            var enumerableField = recordForm.GetEnumerableFieldViewModel(subGridReference);
            if (enumerableField.Enumerable == null)
                throw new NullReferenceException("Error the selectable property is null");
            foreach(var item in enumerableField.Enumerable)
            {
                options.Add(GetAsPicklistOption(item));
            }
            return options;
        }

        private static PicklistOption GetAsPicklistOption(object item)
        {
            object keyValue = GetKeyValue(item);
            var option = new PicklistOption(keyValue.ToString(), item.ToString());
            return option;
        }

        private static string GetKeyValue(object item)
        {
            var keyProperties = item.GetType().GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null);
            if (!keyProperties.Any())
                throw new NullReferenceException($"Could not find property with {typeof(KeyAttribute).Name} attribute on type {item.GetType().Name}");
            var keyProperty = keyProperties.First();
            var keyValue = item.GetPropertyValue(keyProperty.Name);
            if (keyValue == null)
            {
                throw new NullReferenceException($"Error key property is null on item {item.ToString()}");
            }

            return keyValue.ToString();
        }

        public override IEnumerable<PicklistOption> GetInitialSelectedOptions(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var options = new List<PicklistOption>();
            var enumerableField = recordForm.GetEnumerableFieldViewModel(subGridReference);

            if (enumerableField.Enumerable != null)
            {
                foreach (var item in enumerableField.Enumerable)
                {
                    if (!(item is ISelectable))
                        throw new Exception($"Error the type of object {item.GetType().Name} does not implemented {typeof(ISelectable).Name}");
                    if (((ISelectable)item).Selected)
                    {
                        options.Add(GetAsPicklistOption(item));
                    }
                }
            }

            return options;
        }

        public override void AddSelectedItems(IEnumerable<PicklistOption> selectedItems, RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var enumerableField = recordForm.GetEnumerableFieldViewModel(subGridReference);
            if (enumerableField.Enumerable == null)
                throw new NullReferenceException("Error the selectable property is null");
            foreach (var item in enumerableField.Enumerable)
            {
                var key = GetKeyValue(item);
                var asSelectable = item as ISelectable;
                if (asSelectable == null)
                    throw new Exception($"Error the type of object {item.GetType().Name} does not implemented {typeof(ISelectable).Name}");
                asSelectable.Selected = selectedItems.Any(p => p.Key == key);
            }
            enumerableField.OnPropertyChanged(nameof(EnumerableFieldViewModel.StringDisplay));
        }
    }
}