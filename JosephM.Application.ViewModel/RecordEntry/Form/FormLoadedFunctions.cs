using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class FormLoadedFunctions
    {
        private List<FormLoadedFunction> _customfunctions;

        public FormLoadedFunctions()
        {
            _customfunctions = new List<FormLoadedFunction>();
        }

        public void AddFunction(FormLoadedFunction customFunction)
        {
            _customfunctions.Add(customFunction);
        }

        public IEnumerable<FormLoadedFunction> CustomFunctions
        {
            get { return _customfunctions; }
        }
    }
}
