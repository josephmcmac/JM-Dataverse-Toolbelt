using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Migration.Prism.Module.Sql
{
    public class OpenSqlRecordViewModel : OpenViewModel
    {
        public OpenSqlRecordViewModel(FormController formController, Action onCancel) : base(formController)
        {
            IsReadOnly = true;
            OnCancel = onCancel;
        }
    }
}
