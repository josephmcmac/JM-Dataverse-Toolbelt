#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Core.Extentions;
using JosephM.Application.ViewModel.HTML;

#endregion

namespace JosephM.Wpf.Html
{
    /// <summary>
    ///     Interaction logic for ApplicatiionOptionsView.xaml
    /// </summary>
    public partial class HtmlFileView : UserControl
    {
        private HtmlFileModel HtmlFileModel
        {
            get { return DataContext as HtmlFileModel; }
        }

        public HtmlFileView()
        {
            InitializeComponent();
        }

        public void OnDataContextChanged(object s, DependencyPropertyChangedEventArgs e)
        {
            if (!HtmlFileModel.FileNameQualified.IsNullOrWhiteSpace())
                this.BrowserControl.Navigate(HtmlFileModel.FileNameQualified);
        }
    }
}