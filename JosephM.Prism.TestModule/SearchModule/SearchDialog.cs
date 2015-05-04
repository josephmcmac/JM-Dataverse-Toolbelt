using System;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.Fakes;
using JosephM.Record.Application.RecordEntry;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Application.Search;
using JosephM.Record.Application.Shared;
using JosephM.Record.Query;
using JosephM.Record.Service;

namespace JosephM.Prism.TestModule.SearchModule
{
    public class SearchDialog : DialogViewModel
    {
        private FakeRecordService FakeRecordService { get; set; }
        private SearchRequest SearchRequest { get; set; }

        public SearchDialog(FakeRecordService service, IDialogController dialogController)
            : base(dialogController)
        {
            FakeRecordService = service;
        }

        protected override void LoadDialogExtention()
        {
            SearchRequest = new SearchRequest();
            var recordService = new ObjectRecordService(SearchRequest, FakeRecordService, null);
            var formService = new ObjectFormService(SearchRequest, recordService);
            var searchEntryViewModel = new SearchEntryViewModel(LoadSearchResults, OnCancel, SearchRequest,
                new FormController(recordService, formService, ApplicationController));
            Controller.LoadToUi(searchEntryViewModel);
        }

        private SearchResultViewModel SearchResultViewModel { get; set; }

        public void LoadSearchResults()
        {
            DoWhileLoading(() =>
            {
                if (SearchResultViewModel != null)
                    Controller.RemoveFromUi(SearchResultViewModel);
                SearchResultViewModel = null;
                var loadingViewModel = new LoadingViewModel(ApplicationController);
                Controller.LoadToUi(loadingViewModel);

                var condition =
                    SearchRequest.SearchType == SearchType.Contains
                        ? new Condition(SearchRequest.FieldToSearch.Key, ConditionType.Equal,
                            SearchRequest.SearchValue)
                        : new Condition(SearchRequest.FieldToSearch.Key, ConditionType.Like,
                            "%" + SearchRequest.SearchValue + "%");
                var records = FakeRecordService.RetrieveAllAndClauses(SearchRequest.RecordType.Key,
                    new[] {condition});
                //need to wrap in an irecord and irecordservice and display in readonly search result grid
                //try add an open option
                SearchResultViewModel = new SearchResultViewModel(records, FakeRecordService, ApplicationController, -1);
                Controller.RemoveFromUi(loadingViewModel);
                Controller.LoadToUi(SearchResultViewModel);
            });
        }

        private void OnSearchResultDoubleClick()
        {
            var selected = SearchResultViewModel.SelectedRow;
            var record = selected.GetRecord();
            var id = FakeRecordService.GetPrimaryKey(record.Type);
            ApplicationController.OpenRecord(record.Type, id, record.Id, typeof (FakeMaintainViewModel));
        }

        protected override void CompleteDialogExtention()
        {
            //do nothing
        }
    }
}