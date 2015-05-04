#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;
using JosephM.Record.IService;

#endregion

namespace JosephM.Record.Application.RecordEntry
{
    public class FormInstanceBase
    {
        protected IRecordService RecordService { get; private set; }
        protected RecordEntryFormViewModel RecordForm { get; private set; }

        protected IEnumerable<FieldViewModelBase> RecordFields
        {
            get { return RecordForm.FieldViewModels; }
        }

        public static FormInstanceBase Factory(Type formInstanceType, IRecordService recordService,
            RecordEntryFormViewModel recordForm)
        {
            if (!formInstanceType.IsTypeOf(typeof (FormInstanceBase)))
                throw new ArgumentOutOfRangeException("formInstanceType", string.Format("Must Be A Type Of {0}",
                    typeof (FormInstanceBase).Name));
            if (!formInstanceType.HasParameterlessConstructor())
                throw new Exception(string.Format("type {0} must have a parameterless constuctor", formInstanceType.Name));
            var instance = (FormInstanceBase) formInstanceType.CreateFromParameterlessConstructor();
            instance.RecordService = recordService;
            instance.RecordForm = recordForm;
            return instance;
        }

        public void OnChange(string fieldName)
        {
            OnChangeExtention(fieldName);
        }

        public virtual bool OnSaveConfirmation()
        {
            return true;
        }

        protected virtual void OnChangeExtention(string fieldName)
        {
        }

        public void OnLoad(RecordEntryFormViewModel recordForm)
        {
            RecordForm = recordForm;
            OnLoadExtention();
        }

        protected virtual void OnLoadExtention()
        {
        }

        public FieldViewModelBase GetField(string fieldName)
        {
            if (RecordFields.Any(f => f.FieldName == fieldName))
                return RecordFields.First(f => f.FieldName == fieldName);
            throw new ArgumentOutOfRangeException("fieldName",
                string.Concat("There is no field with the specfied name of ",
                    fieldName));
        }

        public object GetFieldValue(string fieldName)
        {
            return GetField(fieldName).ValueObject;
        }

        public object SetFieldValue(string fieldName, object fieldValue)
        {
            return GetField(fieldName).ValueObject = fieldValue;
        }

        protected void UserMessage(string message)
        {
            RecordForm.UserMessage(message);
        }
    }
}