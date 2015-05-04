#region

using System;
using System.Linq;
using JosephM.Record.Application.RecordEntry;

#endregion

namespace JosephM.Record.Application.Fakes
{
    public class FakeFormInstance : FormInstanceBase
    {
        protected override void OnChangeExtention(string fieldName)
        {
            switch (fieldName)
            {
                case FakeConstants.DateOfBirthField:
                {
                    CalculateAge();
                    break;
                }
                case FakeConstants.BooleanField:
                {
                    var boolValue =
                        (bool?) RecordFields.First(fv => fv.FieldName == FakeConstants.BooleanField).ValueObject;
                    RecordFields.First(fv => fv.FieldName == FakeConstants.ComboField).IsEditable =
                        boolValue.HasValue && boolValue.Value;
                    RecordFields.First(fv => fv.FieldName == FakeConstants.StringField).IsVisible =
                        boolValue.HasValue && boolValue.Value;
                    break;
                }
            }
        }

        private void CalculateAge()
        {
            var dobField = RecordFields.Single(vm => vm.FieldName == FakeConstants.DateOfBirthField);
            var dob = (DateTime?) dobField.ValueObject;
            var ageField = RecordFields.Single(vm => vm.FieldName == FakeConstants.AgeField);
            ageField.ValueObject = dob.HasValue ? GetAge(dob) : null;
        }

        protected override void OnLoadExtention()
        {
            CalculateAge();
        }

        public static int? GetAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return null;

            var today = DateTime.Today;
            if (dateOfBirth.Value.Month > today.Month ||
                (dateOfBirth.Value.Month == today.Month && dateOfBirth.Value.Day > today.Day))
                return today.Year - dateOfBirth.Value.Year - 1;
            return today.Year - dateOfBirth.Value.Year;
        }
    }
}