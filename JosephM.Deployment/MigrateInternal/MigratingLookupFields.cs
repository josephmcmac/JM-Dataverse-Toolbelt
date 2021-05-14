using System.Collections.Generic;
using System.ComponentModel;

namespace JosephM.Deployment.MigrateInternal
{
    public class MigratingLookupFields : INotifyPropertyChanged
    {
        public MigratingLookupFields(IEnumerable<MigratedLookupField> fieldsForMigation)
        {
            FieldsForMigation = fieldsForMigation;
        }

        public IEnumerable<MigratedLookupField> FieldsForMigation { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}