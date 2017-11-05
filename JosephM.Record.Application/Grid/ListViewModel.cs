using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using JosephM.Application.Application;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry;
using JosephM.Record.IService;
using JosephM.Record.Metadata;

namespace JosephM.Application.ViewModel.Grid
{
    public class ListViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ListViewModel(string recordType, IRecordService recordService, IApplicationController controller, Action<GridRowViewModel> editGridRow, Action addRow, IEnumerable<CustomGridFunction> customFunctions)
            : base(controller)
        {
            DynamicGridViewModel = new DynamicGridViewModel(ApplicationController)
            {
                EditRow = editGridRow,
                AddRow = addRow,
                PageSize = StandardPageSize,
                ViewType = ViewType.MainApplicationView,
                RecordService = recordService,
                RecordType = recordType,
                IsReadOnly = true,
                FormController = new FormController(recordService, null, controller),
                GetGridRecords = GetGridRecords
            };
            DynamicGridViewModel.AddGridButtons(customFunctions);
            DynamicGridViewModel.ReloadGrid();
        }


        private string _heading = "List View";
        public string Heading { get { return _heading; }}

        public void DoWhileLoading(string message, Action action)
        {
            action();
        }

        public DynamicGridViewModel DynamicGridViewModel { get; private set; }

        public void LoadChildForm(ViewModelBase viewModel)
        {
            ApplicationController.DoOnMainThread(() =>
            {
                ChildForms.Add(viewModel);
                OnPropertyChanged("MainFormInContext");
            });
        }

        public void ClearChildForm()
        {
            ApplicationController.DoOnMainThread(() =>
            {
                ChildForms.Clear();
                OnPropertyChanged("MainFormInContext");
            });
        }

        private ObservableCollection<ViewModelBase> _childForms = new ObservableCollection<ViewModelBase>();

        /// <summary>
        /// DONT USE CLEAR USER ClearChildForm()
        /// </summary>
        public ObservableCollection<ViewModelBase> ChildForms
        {
            get { return _childForms; }
            set
            {
                _childForms = value;
                OnPropertyChanged("ChildForms");
                OnPropertyChanged("MainFormInContext");
            }
        }

        public bool MainFormInContext
        {
            get
            {
                return !ChildForms.Any();
            }
        }

        public ObservableCollection<GridRowViewModel> GridRecords
        {
            get { return DynamicGridViewModel.GridRecords; }
        }

        public GetGridRecordsResponse GetGridRecords(bool ignorePages)
        {
            var view = DynamicGridViewModel.RecordService.GetView(DynamicGridViewModel.RecordType, DynamicGridViewModel.ViewType);
            if (!DynamicGridViewModel.HasPaging)
            {
                var records =
                    DynamicGridViewModel.RecordService.GetFirstX(DynamicGridViewModel.RecordType, -1, null, null,
                        view.Sorts);
                return new GetGridRecordsResponse(records);
            }
            else
            {
                return DynamicGridViewModel.GetGridRecordPage(null, view.Sorts);
            }
        }
    }
}
