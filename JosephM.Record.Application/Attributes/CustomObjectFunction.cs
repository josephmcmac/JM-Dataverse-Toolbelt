using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Extentions;
using System;

namespace JosephM.Application.ViewModel.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class CustomObjectFunction : CustomFunction
    {
        public string FunctionName { get; set; }

        public CustomObjectFunction(string functionName)
        {
            FunctionName = functionName;
        }

        public override string GetFunctionLabel()
        {
            return FunctionName.SplitCamelCase();
        }


        public override Action GetCustomFunction(RecordEntryViewModelBase recordForm, string subGridReference)
        {
            var objectFormService = recordForm as ObjectEntryViewModel;
            if (objectFormService == null)
            {
                throw new ArgumentOutOfRangeException(nameof(recordForm), string.Format("Required To Be Type Of {0}. Actual Type Is {1}", typeof(ObjectEntryViewModel).Name, recordForm.GetType().Name));
            }
            var objectToEnter = objectFormService.GetObject();
            var thisMethod = new Action(() => objectToEnter.InvokeMethod(FunctionName, recordForm.ApplicationController));
            return () =>
                {
                    objectFormService.LoadSubgridsToObject();
                    thisMethod();
                    var subGrid = objectFormService.GetSubGridViewModel(subGridReference);
                    subGrid.DynamicGridViewModel.ReloadGrid();
                };
        }
    }
}