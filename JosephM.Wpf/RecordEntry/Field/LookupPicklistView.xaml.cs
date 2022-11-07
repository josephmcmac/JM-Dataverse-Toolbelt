using Binding = System.Windows.Data.Binding;

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class LookupPicklistView : FieldControlBase
    {
        public LookupPicklistView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return null;
        }
    }
}