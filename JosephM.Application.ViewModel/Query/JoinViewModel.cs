using JosephM.Application.Application;
using JosephM.Application.ViewModel.Shared;
using JosephM.Core.FieldType;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Application.ViewModel.Query
{
    public class JoinViewModel : ViewModelBase
    {
        public JoinViewModel(string recordType, IRecordService recordService, IApplicationController controller, Action onPopulated, Action<JoinViewModel> remove, Action onConditionSelectedChanged)
            : base(controller)
        {
            OnConditionSelectedChanged = onConditionSelectedChanged;
            RecordType = recordType;
            RecordService = recordService;
            //need to somehow have a relationship selections
            LinkSelections = GetSelections();
            OnPopulated = onPopulated;
            Remove = new MyCommand(() => remove(this));
            LoadingViewModel = new LoadingViewModel(ApplicationController);
        }

        private IEnumerable<PicklistOption> GetSelections()
        {
            if (RecordType == null)
                return new PicklistOption[0];

            //need to generate a list of all join optionds
            //n:N, 1:N or N:1

            var options = new List<PicklistOption>();
            var lookupFields = RecordService.GetFieldMetadata(RecordType)
                .Where(f => f.FieldType == Record.Metadata.RecordFieldType.Lookup || f.FieldType == Record.Metadata.RecordFieldType.Customer)
                .ToArray();
            //okay these need appending for each target type
            foreach(var lookupField in lookupFields)
            {
                var targetTypes = RecordService.GetLookupTargetType(lookupField.SchemaName, RecordType);
                if(!string.IsNullOrWhiteSpace(targetTypes))
                {
                    var relationshipLabel = lookupField.DisplayName;
                    var split = targetTypes.Split(',');
                    var isMultiple = split.Count() > 1;
                    foreach(var target in split)
                    {
                        var key = $"n1:{lookupField.SchemaName}:{target}";
                        var label = relationshipLabel + (isMultiple ? ($" ({RecordService.GetDisplayName(target)})") : null);
                        var option = new PicklistOption(key, label);
                        options.Add(option);
                    }
                }
            }

            var manyToManyRelationships = RecordService.GetManyToManyRelationships(RecordType).ToArray();
            foreach(var manyToMany in manyToManyRelationships)
            {
                if(manyToMany.RecordType1 == RecordType)
                {
                    var key = $"nn:{manyToMany.IntersectEntityName}:{manyToMany.Entity1IntersectAttribute}:{manyToMany.Entity2IntersectAttribute}:{manyToMany.RecordType2}";
                    var label = manyToMany.RecordType2UseCustomLabel ? manyToMany.RecordType2CustomLabel : RecordService.GetDisplayName(manyToMany.RecordType2);
                    var option = new PicklistOption(key, label);
                    options.Add(option);
                }
                if (manyToMany.RecordType2 == RecordType)
                {
                    var key = $"nn:{manyToMany.IntersectEntityName}:{manyToMany.Entity2IntersectAttribute}:{manyToMany.Entity1IntersectAttribute}:{manyToMany.RecordType1}";
                    var label = manyToMany.RecordType1UseCustomLabel ? manyToMany.RecordType1CustomLabel : RecordService.GetDisplayName(manyToMany.RecordType1);
                    var option = new PicklistOption(key, label);
                    options.Add(option);
                }
            }

            var oneToManyRelationships = RecordService.GetOneToManyRelationships(RecordType).ToArray();
            foreach(var oneToMany in oneToManyRelationships)
            {
                if (RecordService.FieldExists(oneToMany.ReferencingAttribute, oneToMany.ReferencingEntity))
                {
                    var key = $"1n:{oneToMany.ReferencingEntity}:{oneToMany.ReferencingAttribute}";
                    var label = $"{RecordService.GetDisplayName(oneToMany.ReferencingEntity)} ({RecordService.GetFieldLabel(oneToMany.ReferencingAttribute, oneToMany.ReferencingEntity)})";
                    var option = new PicklistOption(key, label);
                    options.Add(option);
                }
            }

            options.Sort((p1, p2) => p1.CompareTo(p2));
            options = options.Distinct().ToList();
            return options;
        }

        public bool Validate()
        {
            var result = true;
            if (FilterConditions != null)
            {
                result = FilterConditions.Validate();
            }
            return result;
        }

        public Action OnConditionSelectedChanged { get; private set; }
        private string RecordType { get; set; }

        private IRecordService RecordService { get; set; }

        private PicklistOption _selectedItem;
        public PicklistOption SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                var isBeingPopulated = _selectedItem == null;
                var isChanging = _selectedItem != value;
                _selectedItem = value;
                if(isBeingPopulated && OnPopulated != null)
                {
                    OnPopulated();
                }
                if (isChanging && value != null)
                {
                    DoOnAsynchThread(() =>
                    {
                        try
                        {
                            LoadingViewModel.LoadingMessage = $"Loading {RecordService.GetDisplayName(SelectedRelationshipTarget)} Fields";
                            LoadingViewModel.IsLoading = true;
                            FilterConditions = CreateFilterCondition();
                            Joins = new JoinsViewModel(SelectedRelationshipTarget, RecordService, ApplicationController, OnConditionSelectedChanged);
                        }
                        catch(Exception ex)
                        {
                            ApplicationController.ThrowException(ex);
                        }
                        finally
                        {
                            LoadingViewModel.IsLoading = false;
                        }
                    });
                }
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private string SelectedRelationshipTarget
        {
            get
            {
                if (SelectedItem == null)
                    return null;
                return new ParsedSelection(SelectedItem.Key, RecordType, RecordService).OtherType;
            }
        }

        public IEnumerable<PicklistOption> LinkSelections { get; set; }
        public Action OnPopulated { get; }
        public MyCommand Remove { get; }
        public LoadingViewModel LoadingViewModel { get; private set; }

        private JoinsViewModel _joins;
        public JoinsViewModel Joins
        {
            get
            {
                return _joins;
            }
            set
            {
                _joins = value;
                OnPropertyChanged(nameof(Joins));
            }
        }

        private FilterConditionsViewModel CreateFilterCondition()
        {
            return new FilterConditionsViewModel(SelectedRelationshipTarget, RecordService, ApplicationController, OnConditionSelectedChanged);
        }


        private FilterConditionsViewModel _filterConditions;
        public FilterConditionsViewModel FilterConditions
        {
            get
            {
                return _filterConditions;
            }
            set
            {
                _filterConditions = value;
                OnPropertyChanged(nameof(FilterConditions));
            }
        }

        public Join GetAsJoin()
        {
            var selected = SelectedItem?.Key;
            if (selected == null)
                return null;
            Join join = new ParsedSelection(selected, RecordType, RecordService).GetAsJoin();
            join.RootFilter = FilterConditions.GetAsFilter();
            var childJoins = Joins != null ? Joins.GetAsJoins() : new Join[0];
            join.Joins = childJoins.ToList();
            return join;
        }

        private class ParsedSelection
        {
            public ParsedSelection(string selectedKey, string sourceType, IRecordService recordService)
            {
                SourceType = sourceType;
                RecordService = recordService;
                if (selectedKey != null)
                {
                    var splitIt = selectedKey.Split(':');
                    RelationshipType = splitIt[0];
                    if (RelationshipType == "nn")
                    {
                        IntersectEntity = splitIt[1];
                        IntersectJoinTo = splitIt[2];
                        IntersectOtherSide = splitIt[3];
                        OtherType = splitIt[4];
                    }
                    if (RelationshipType == "1n")
                    {
                        FieldJoinTo = splitIt[2];
                        OtherType = splitIt[1];
                    }
                    if (RelationshipType == "n1")
                    {
                        FieldJoin = splitIt[1];
                        OtherType = splitIt[2];
                    }
                }
            }

            public Join GetAsJoin()
            {
                if (RelationshipType == "nn")
                {
                    var joinThrough = new Join(RecordService.GetPrimaryKey(SourceType), IntersectEntity, IntersectJoinTo);
                    var join = new Join(IntersectOtherSide, OtherType, RecordService.GetPrimaryKey(OtherType));
                    joinThrough.Joins = new List<Join> { join };
                    return joinThrough;
                }
                if (RelationshipType == "1n")
                {
                    return new Join(RecordService.GetPrimaryKey(SourceType), OtherType, FieldJoinTo);
                }
                if (RelationshipType == "n1")
                {
                    return new Join(FieldJoin, OtherType, RecordService.GetPrimaryKey(OtherType));
                }
                return null;
            }

            public string RelationshipType { get; private set; }

            public string SourceType { get; private set; }
            public IRecordService RecordService { get; }
            public string IntersectEntity { get; private set; }
            public string IntersectJoinTo { get; private set; }
            public string IntersectOtherSide { get; private set; }
            public string OtherType { get; private set; }
            public string FieldJoinTo { get; private set; }
            public string FieldJoin { get; private set; }
        }

        public void DeleteSelectedConditions()
        {
            FilterConditions?.DeleteSelected(null);
            if (Joins != null && Joins.Joins != null)
            {
                foreach (var join in Joins.Joins)
                {
                    join.DeleteSelectedConditions();
                }
            }
        }

        public void UngroupSelectedConditions()
        {
            FilterConditions?.UnGroupSelected(null);
            if (Joins != null && Joins.Joins != null)
            {
                foreach (var join in Joins.Joins)
                {
                    join.UngroupSelectedConditions();
                }
            }
        }

        public void GroupSelected(FilterOperator filterOperator)
        {
            FilterConditions?.GroupSelected(filterOperator);
            if (Joins != null && Joins.Joins != null)
            {
                foreach (var join in Joins.Joins)
                {
                    join.GroupSelected(filterOperator);
                }
            }
        }
    }
}
