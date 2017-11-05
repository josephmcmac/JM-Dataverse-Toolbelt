#region

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using JosephM.Wpf.RecordEntry.Field;

#endregion

namespace JosephM.Wpf.Grid
{
    /// <summary>
    ///     Interaction logic for SubGrid.xaml
    /// </summary>
    public partial class GridMultiSelectFieldView : FieldControlBase
    {
        public GridMultiSelectFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return null;
        }
    }
}