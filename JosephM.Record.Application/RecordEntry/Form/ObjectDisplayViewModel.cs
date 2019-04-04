using JosephM.Record.IService;
using JosephM.Record.Service;
using System;

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class ObjectDisplayViewModel : RecordEntryFormViewModel
    {
        private readonly ObjectRecord _objectRecord;

        public override int GridPageSize { get { return 25; } }

        public ObjectDisplayViewModel(object objectToEnter, FormController formController, Action nextAction = null, string nextActionLabel = null, Action cancelAction = null, Action backAction = null)
            : base(formController, saveButtonLabel: nextActionLabel ?? "Next")
        {
            IsReadOnly = true;
            OnSave = nextAction;
            OnBack = backAction;
            OnCancel = cancelAction;

            _objectRecord = new ObjectRecord(objectToEnter);
            RecordType = _objectRecord.Type;
        }

        public object GetObject()
        {
            return _objectRecord.Instance;
        }

        public override IRecord GetRecord()
        {
            return _objectRecord;
        }

        public override void LoadFormSections()
        {
            base.LoadFormSections();
            foreach (var grid in SubGrids)
            {
                grid.DynamicGridViewModel.ReloadGrid();
            }
        }

        internal override void RefreshEditabilityExtention()
        {
            base.RefreshEditabilityExtention();
        }

        public ObjectRecordService GetObjectRecordService()
        {
            var service = RecordService;
            if (!(service is ObjectRecordService))
                throw new TypeLoadException(string.Format("Expected {0} Of Type {1}", typeof(IRecordService).Name,
                    typeof(ObjectRecordService).Name));
            return ((ObjectRecordService)service);
        }

        public override string Instruction => GetObjectRecordService().GetInstruction(RecordType);
    }
}