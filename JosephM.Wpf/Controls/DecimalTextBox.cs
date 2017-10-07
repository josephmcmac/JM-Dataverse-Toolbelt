#region

using System.Text.RegularExpressions;
using JosephM.Core.Constants;

#endregion

namespace JosephM.Wpf.Controls
{
    public class DecimalTextBox : MaskedTextBox
    {
        protected override Regex Regex
        {
            get { return new Regex(RegularExpressions.Decimal); }
        }
    }
}