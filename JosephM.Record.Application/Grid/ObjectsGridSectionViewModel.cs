#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.Extentions;
using JosephM.Core.Log;
using JosephM.Core.Utility;

#endregion

namespace JosephM.Application.ViewModel.Grid
{
    /// <summary>
    ///     For Displaying A Set Of CLR Objects In A DataGrid With The Standard .NET Runtime Auto-generated ColumnsStandard
    ///     Includes Function For Downloading The Data In The Objects To A CSV File
    /// </summary>
    public class ObjectsGridSectionViewModel : ViewModelBase
    {
        public ObjectsGridSectionViewModel(string heading, IEnumerable<object> items, IApplicationController controller)
            : base(controller)
        {
            Items = items ?? new object[0];
            HeadingViewModel = new HeadingViewModel(heading, ApplicationController);
        }

        public IEnumerable<object> Items { get; private set; }

        public HeadingViewModel HeadingViewModel { get; private set; }

        private bool _csvForExtraction = true;

        public bool CsvForExtraction
        {
            get { return _csvForExtraction; }
            set
            {
                _csvForExtraction = value;
                OnPropertyChanged("CsvForExtraction");
            }
        }

        private bool _csvExtracted;

        public bool CsvExtracted
        {
            get { return _csvExtracted; }
            set
            {
                _csvExtracted = value;
                OnPropertyChanged("CsvExtracted");
            }
        }

        private bool _csvExtracting;

        public bool CsvExtracting
        {
            get { return _csvExtracting; }
            set
            {
                _csvExtracting = value;
                OnPropertyChanged("CsvExtracting");
            }
        }

        private ProgressControlViewModel _csvExtractingProgress;

        public ProgressControlViewModel CsvExtractingProgress
        {
            get { return _csvExtractingProgress; }
            set
            {
                _csvExtractingProgress = value;
                OnPropertyChanged("CsvExtractingProgress");
            }
        }

        private XrmButtonViewModel _openCsvButton;

        public XrmButtonViewModel OpenCsvButton
        {
            get { return _openCsvButton; }
            set
            {
                _openCsvButton = value;
                OnPropertyChanged("OpenCsvButton");
            }
        }

        private XrmButtonViewModel _openCsvFolderButton;

        public XrmButtonViewModel OpenCsvFolderButton
        {
            get { return _openCsvFolderButton; }
            set
            {
                _openCsvFolderButton = value;
                OnPropertyChanged("OpenCsvFolderButton");
            }
        }

        public void DownloadCsv(string folder)
        {
            ApplicationController.DoOnAsyncThread(() =>
            {
                CsvForExtraction = false;
                CsvExtracting = true;
                CsvExtractingProgress = new ProgressControlViewModel(ApplicationController);
                CsvExtractingProgress.UpdateProgress(0, 1, "Extracting To CSV");
                try
                {
                    var fileName = string.Format("{0} CSV Download - {1}.csv", ApplicationController.ApplicationName,
                        DateTime.Now.ToFileTime());
                    CsvUtility.CreateCsv(folder, fileName
                        , Items, new LogController(CsvExtractingProgress));
                    CsvExtracting = false;
                    CsvExtracted = true;
                    OpenCsvButton = new XrmButtonViewModel("Open CSV", () => OpenFile(folder, fileName),
                        ApplicationController);
                    OpenCsvFolderButton = new XrmButtonViewModel("Open CSV Folder", () => OpenFolder(folder),
                        ApplicationController);
                }
                catch (Exception ex)
                {
                    CsvExtracting = false;
                    CsvForExtraction = true;
                    ApplicationController.UserMessage("Error Downloading CSV: " + ex.DisplayString());
                }
            });
        }

        public void OpenFile(string folder, string name)
        {
            try
            {
                Process.Start(folder + "\\" + name);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }

        public void OpenFolder(string folder)
        {
            try
            {
                Process.Start(folder);
            }
            catch (Exception ex)
            {
                ApplicationController.UserMessage(ex.DisplayString());
            }
        }
    }
}