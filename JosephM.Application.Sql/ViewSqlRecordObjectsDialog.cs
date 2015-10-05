using System;
using System.Reflection;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Attributes;
using JosephM.Record.Sql;

namespace JosephM.Migration.Prism.Module.Sql
{
    public abstract class ViewSqlRecordObjectsDialog<T> : ViewSqlRecordsDialog
        where T : new()
    {
        protected ViewSqlRecordObjectsDialog(ISqlRecordMetadataService recordService, IDialogController dialogController)
            : base(recordService, dialogController)
        {
            RecordService = recordService;
        }

        protected T LoadObject(string id)
        {
            return RecordService.LoadToObject<T>(id);
        }

        private Type ObjectType
        {
            get { return typeof (T); }
        }

        protected override string RecordType
        {
            get
            {
                if (RecordTypeMap == null)
                    throw new NullReferenceException(string.Format("Type {0} does not have a {1} attribute",
                        ObjectType.Name, RecordTypeMap.GetType().Name));
                return RecordTypeMap.RecordType;
            }
        }

        public RecordTypeMap RecordTypeMap
        {
            get
            {
                var recordTypeMap = ObjectType.GetCustomAttribute<RecordTypeMap>(true);
                if(recordTypeMap == null)
                    throw new NullReferenceException(string.Format("Type {0} does not have {1} attribute", ObjectType.Name, typeof(RecordTypeMap).Name));
                return recordTypeMap;
            }
        }
    }
}