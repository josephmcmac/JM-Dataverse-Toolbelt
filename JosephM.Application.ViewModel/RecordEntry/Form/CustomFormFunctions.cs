using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class CustomFormFunctions
    {
        private List<CustomFormFunction> _customfunctions;

        public CustomFormFunctions()
        {
            _customfunctions = new List<CustomFormFunction>();
        }

        public void AddFunction(CustomFormFunction customFunction)
        {
            _customfunctions.Add(customFunction);
        }

        public IEnumerable<CustomFormFunction> CustomFunctions
        {
            get { return _customfunctions; }
        }
    }
}
