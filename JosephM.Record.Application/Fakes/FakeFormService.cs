#region

using System;
using JosephM.Record.Application.RecordEntry.Metadata;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Record.Application.Fakes
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
                        new NonPersistentFormField(FakeConstants.AgeField, "Age",
                            RecordFieldType.Integer)
                    });
            }
        }

        public static SubGridSection FakeSubGridSection
        {
            get
            {
                return new SubGridSection("Section 3",
                    FakeConstants.RecordType,
                    FakeConstants.LookupField,
                    FakeConstants.FakeGridFields
                    );
            }
        }

        public override FormMetadata GetFormMetadata(string recordType)
        {
            return new FormMetadata(
                new FormSection[]
                {
                    FakeSection1, FakeSection2, FakeSubGridSection
                });
        }

        protected override Type GetFormInstanceType(string recordType)
        {
            return typeof (FakeFormInstance);
        }
    }
}