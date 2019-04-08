using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class OnChangeFunctions
    {
        private List<OnChangeFunction> _customfunctions;

        public OnChangeFunctions()
        {
            _customfunctions = new List<OnChangeFunction>();
        }

        public void AddFunction(OnChangeFunction customFunction)
        {
            _customfunctions.Add(customFunction);
        }

        public IEnumerable<OnChangeFunction> CustomFunctions
        {
            get { return _customfunctions; }
        }
    }
}
