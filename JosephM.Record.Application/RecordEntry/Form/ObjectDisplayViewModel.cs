#region

using System;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Record.Application.RecordEntry.Section;
using JosephM.Record.IService;
using JosephM.Record.Service;

#endregion

namespace JosephM.Record.Application.RecordEntry.Form
{
    public class ObjectDisplayViewModel : RecordEntryFormViewModel
    {
        private readonly ObjectRecord _objectRecord;

        public ObjectDisplayViewModel(object objectToEnter, FormController formController)
            : base(formController)
        {
            _objectRecord = new ObjectRecord(objectToEnter);
            RecordType = _objectRecord.Type;
        }

        protected object GetObject()
        {
            return _objectRecord.Instance;
        }

        public override IRecord GetRecord()
        {
            return _objectRecord;
        }

        public override bool ShowSaveButton
        {
            get { return false; }
        }

        public override bool ShowCancelButton
        {
            get { return false; }
        }
    }
}