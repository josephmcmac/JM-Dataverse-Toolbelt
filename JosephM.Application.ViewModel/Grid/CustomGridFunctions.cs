using JosephM.Application.ViewModel.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace JosephM.Application.ViewModel.Grid
{
    public class CustomGridFunctions
    {
        private List<CustomGridFunction> _customfunctions;

        public CustomGridFunctions()
        {
            _customfunctions = new List<CustomGridFunction>();
        }

        public void AddFunction(CustomGridFunction customFunction)
        {
            _customfunctions.Add(customFunction);
        }

        public IEnumerable<CustomGridFunction> CustomFunctions
        {
            get { return _customfunctions; }
        }
    }
}
