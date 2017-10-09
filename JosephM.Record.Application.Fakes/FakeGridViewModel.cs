#region

using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;
using System.Collections.Generic;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeGridViewModel : DynamicGridViewModel
    {
        public FakeGridViewModel()
            : base(new FakeApplicationController())
        {
            PageSize = 50;
            ViewType = Record.Metadata.ViewType.MainApplicationView;
            RecordService = FakeRecordService.Get();
            RecordType = FakeConstants.RecordType;
            IsReadOnly = true;
            FormController = new FakeFormController();
            GetGridRecords = (b) => { return new GetGridRecordsResponse(RecordService.RetrieveAll(RecordType, null)); };
            MultiSelect = true;
            GridLoaded = false;
            //var customFunctions = new List<CustomGridFunction>()
            //{
            //    new CustomGridFunction("Dummy", () => { })
            //};

            //LoadGridButtons(customFunctions);

            ReloadGrid();
        }
    }
}