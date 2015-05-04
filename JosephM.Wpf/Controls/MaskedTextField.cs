#region

using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

#endregion

namespace JosephM.Wpf.Controls
{
    public abstract class MaskedTextBox : TextBox
    {
        private string _oldValue;

        protected MaskedTextBox()
        {
            TextChanged += OnTextChanged;
        }

        protected abstract Regex Regex { get; }

        private void OnTextChanged(object sender, TextChangedEventArgs args)
        {
            var tempCaretIndex = CaretIndex;
            if (!String.IsNullOrEmpty(Text) && Text != "-")
            {
                if (!Regex.IsMatch(Text))
                {
                    Text = _oldValue;
                    CaretIndex = Text == null
                        ? 0
                        : (tempCaretIndex > Text.Length ? Text.Length : tempCaretIndex);
                    return;
                }
            }
            _oldValue = Text;
        }
    }
}