#region

using System.ComponentModel;
using JosephM.Record.IService;
using JosephM.Record.Service;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Form
{
    public class ObjectDisplayViewModel : RecordEntryFormViewModel
    {
        private readonly ObjectRecord _objectRecord;

        public override int GridPageSize { get { return 25; } }

        public ObjectDisplayViewModel(object objectToEnter, FormController formController)
            : base(formController)
        {
            IsReadOnly = true;

            _objectRecord = new ObjectRecord(objectToEnter);
            RecordType = _objectRecord.Type;

            if (objectToEnter.GetType().IsTypeOf(typeof(INotifyPropertyChanged)))
            {
                var iNotify = (INotifyPropertyChanged)objectToEnter;
                iNotify.PropertyChanged += NotifyOnPropertyChanged;
            }
        }

        protected object GetObject()
        {
            return _objectRecord.Instance;
        }

        public override IRecord GetRecord()
        {
            return _objectRecord;
        }

        private void NotifyOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            DoOnMainThread(() =>
            {
                var fieldViewModel = GetFieldViewModel(propertyChangedEventArgs.PropertyName);
                fieldViewModel.OnChangeBase();
            });
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
    }
}