using JosephM.Application.Application;
using JosephM.Record.IService;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JosephM.Application.ViewModel.Query
{
    public class JoinsViewModel : ViewModelBase
    {
        public JoinsViewModel(string recordType, IRecordService recordService, IApplicationController controller, Action onConditionSelectedChanged)
            : base(controller)
        {
            OnConditionSelectedChanged = onConditionSelectedChanged;
            RecordType = recordType;
            RecordService = recordService;
            Joins = new ObservableCollection<JoinViewModel>();
            AddNewJoin();
        }

        public Action OnConditionSelectedChanged { get; private set; }
        public string RecordType { get; set; }

        private IRecordService RecordService { get; set; }

        private void AddNewJoin()
        {
            var join = new JoinViewModel(RecordType, RecordService, ApplicationController, AddNewJoin, j => Joins.Remove(j), OnConditionSelectedChanged);
            Joins.Add(join);
        }

        public ObservableCollection<JoinViewModel> Joins { get; set; }

        public IEnumerable<Join> GetAsJoins()
        {
            var joins = new List<Join>();
            foreach(var join in Joins)
            {
                var asJoin = join.GetAsJoin();
                if (asJoin != null)
                    joins.Add(asJoin);
            }
            return joins;
        }

        public bool Validate()
        {
            var result = true;
            if (Joins != null)
            {
                foreach (var join in Joins)
                {
                    if (!join.Validate())
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
    }
}
