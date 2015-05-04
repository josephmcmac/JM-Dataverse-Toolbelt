#region

using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    public abstract class FieldControlBase : UserControl
    {
        //protected FieldViewModelBase FieldViewModelBase
        //{
        //    get { return (FieldViewModelBase) DataContext; }
        //}

        //public void OnDataContextChanged(object s, DependencyPropertyChangedEventArgs e)
        //{
        //    Binding binding = GetValidationBinding();
        //    foreach (ValidationRuleBase validationRule in FieldViewModelBase.ValidationRules)
        //        binding.ValidationRules.Add(validationRule);
        //}

        protected abstract Binding GetValidationBinding();
    }
}