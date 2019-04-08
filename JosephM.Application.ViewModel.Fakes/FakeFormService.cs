#region

using JosephM.Application.ViewModel.RecordEntry.Metadata;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using System.Collections.Generic;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeFormService : FormServiceBase
    {
        public static FormFieldSection FakeSection1
        {
            get
            {
                return new FormFieldSection("Main",
                    new FormFieldMetadata[]
                    {
                        new PersistentFormField(FakeConstants.BooleanField),
                        new PersistentFormField(FakeConstants.StringField),
                        new PersistentFormField(FakeConstants.IntegerField),
                        new PersistentFormField(FakeConstants.ComboField),
                        new PersistentFormField(FakeConstants.LookupField)
                    });
            }
        }

        internal static FormFieldSection FakeSection2
        {
            get
            {
                return new FormFieldSection("Section 2",
                    new FormFieldMetadata[]
                    {
                        new PersistentFormField(FakeConstants.DateOfBirthField),
                    });
            }
        }

        internal static FormFieldSection FakeSection3
        {
            get
            {
                var fieldMetadata = new List<PersistentFormField>();
                for (var i = 1; i <= 50; i++)
                    fieldMetadata.Add(new PersistentFormField(FakeConstants.StringField + i));
                return new FormFieldSection("Section 3", fieldMetadata);
            }
        }



        public override FormMetadata GetFormMetadata(string recordType, IRecordService recordService = null)
        {
            if (recordType == FakeConstants.RecordType)
                return new FormMetadata(
                    new FormSection[]
                    {
                    FakeSection1, FakeSection2, FakeSection3
                    });
            else return new FormMetadata(FakeRecordService.Get().GetFields(recordType));
        }
    }
}