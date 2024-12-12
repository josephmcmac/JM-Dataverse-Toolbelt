#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Service;
using JosephM.Record.Extentions;
using JosephM.Core.Extentions;
using JosephM.Core.Test;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public class FakeRecordService : RecordService
    {
        private static FakeRecordService _staticInstance;

        public static FakeRecordService Get()
        {
            if (_staticInstance == null)
                _staticInstance = new FakeRecordService();
            return _staticInstance;
        }

        public IRecord GetMainRecord()
        {
            return this.GetFirst(FakeConstants.RecordType, FakeConstants.PrimaryField, FakeConstants.MainRecordName);
        }

        private FakeRecordService()
            : base(
                GetFakeRecordMetadata(), GetFakeOne2ManyRelationshipMetadata(), GetFakeMany2ManyRelationshipMetadata())
        {
            var mainRecord = CreateFakeMainRecord();
            CreatLinkedRecords(mainRecord);
            var nnNRelationship = GetFakeMany2ManyRelationshipMetadata().First();
            var associated1 = CreateFakeRecord(FakeConstants.RecordType, "Associated 1");
            Associate(nnNRelationship, mainRecord, associated1);
            var associated2 = CreateFakeRecord(FakeConstants.RecordType, "Associated 2");
            Associate(nnNRelationship, mainRecord, associated2);
            CreateFakeRecord(FakeConstants.RecordType2, "I Am A " + FakeConstants.RecordType2);
            CreateFakeRecord(FakeConstants.RecordType2, "Update");
            Create100DummyRecords();
            CreateTextSearchRecords();
            CreateAllFieldRecords();
        }

        private void CreateTextSearchRecords()
        {
            const string testingString = "TestingString";
            IRecord lastPartialMatch = null;
            for (var i = 0; i < 5; i++)
            {
                var matchString = string.Format("I Contain The {0} In String {1}", testingString, i);
                var nonMatchString = string.Format("I Am A String {0}", i);
                CreateFakeMainRecord(matchString, nonMatchString);
                lastPartialMatch = CreateFakeMainRecord(nonMatchString, matchString);
            }
            var exactMatchSearchRecord = CreateFakeMainRecord(testingString, testingString);

            var autoPostText =
                string.Format(
                    "<?xml version='1.0' encoding='utf-16'?><pi id='Contact.Create.Post' icon='0' ya='1'><ps><p type='1' otc='8' id='70777d28-3d98-e311-b769-00155db14b05'>{0}</p></ps></pi>",
                    testingString);
            CreateFakeRecord(FakeConstants.FakePostType, autoPostText);
            var userPostText =
                string.Format("I Posted The Record @[8,70777D28-3D98-E311-B769-00155DB14B05,\"{0}\"] In The Post",
                    testingString);
            CreateFakeRecord(FakeConstants.FakePostType, userPostText);


            var oneNRelationship = GetOneToManyRelationships(FakeConstants.RecordType).First();
            var oneNrecord = CreateFakeMainRecord("I Am 1:N Associated To " + testingString,
                "I Am 1:N Associated " + testingString);
            oneNrecord.SetLookup(oneNRelationship.ReferencingAttribute, exactMatchSearchRecord.Id,
                exactMatchSearchRecord.Type);
            Update(oneNrecord, null);
            if (lastPartialMatch != null)
            {
                var oneNrecord2 =
                    CreateFakeMainRecord(
                        "I Am 1:N Associated to " + lastPartialMatch.GetStringField(FakeConstants.PrimaryField),
                        "I Am 1:N Associated " + lastPartialMatch.GetStringField(FakeConstants.PrimaryField));
                oneNrecord2.SetLookup(oneNRelationship.ReferencingAttribute, lastPartialMatch.Id, lastPartialMatch.Type);
                Update(oneNrecord2, null);
            }
            var nNrecord = CreateFakeMainRecord("I Am N:N Associated", "I Am N:N Associated");
            Associate(GetManyToManyRelationships(FakeConstants.RecordType).First(), exactMatchSearchRecord, nNrecord);


            var contact = CreateFakeRecord(FakeConstants.FakeContactType, testingString + " Contact");
            var contact2 = CreateFakeRecord(FakeConstants.FakeContactType, testingString + " Contact 2");
            var activity = CreateFakeRecord(FakeConstants.FakeEmailType, "This Is An Activity");
            activity.SetField(FakeConstants.FakeEmailTypeContentField, SampleHtml, this);
            Update(activity, null);
            var activityParty = CreateFakeRecord(FakeConstants.FakeActivityPartyType, "Activity Party");
            activityParty.SetField(FakeConstants.FakeActivityPartyTypePartyField, new Lookup(contact.Type, contact.Id, null), this);
            activityParty.SetField(FakeConstants.FakeActivityPartyTypeActivityField, new Lookup(activity.Type, activity.Id, null), this);
            Update(activityParty, null);
            var activityParty2 = CreateFakeRecord(FakeConstants.FakeActivityPartyType, "Activity Party 2");
            activityParty2.SetField(FakeConstants.FakeActivityPartyTypePartyField, new Lookup(contact2.Type, contact2.Id, null), this);
            activityParty2.SetField(FakeConstants.FakeActivityPartyTypeActivityField, new Lookup(activity.Type, activity.Id, null), this);
            Update(activityParty2, null);
            activity.SetField(FakeConstants.FakeEmailTypeActivityParty, new[] {activityParty, activityParty2}, this);
            Update(activity, null);
        }

        private IRecord CreateFakeRecord(string recordType, string primaryField)
        {
            return CreateFakeRecord(recordType, primaryField, null);
        }

        private IRecord CreateFakeRecord(string recordType, string primaryField,
            IDictionary<string, object> additionalFields)
        {
            var id = Guid.NewGuid().ToString();
            var entity = NewRecord(recordType);
            entity.Id = id;
            entity.SetField(this.GetPrimaryKey(recordType), id, this);
            entity.SetField(this.GetPrimaryField(recordType), primaryField, this);
            if (additionalFields != null)
            {
                foreach (var field in additionalFields)
                {
                    entity.SetField(field.Key, field.Value, this);
                }
            }
            return Get(entity.Type, Create(entity, null));
        }

        public IRecord CreateFakeMainRecord(string name, string stringField)
        {
            var additionalFields = new Dictionary<string, object>()
            {
                {
                    FakeConstants.StringField,
                    stringField
                }
            };
            return CreateFakeRecord(FakeConstants.RecordType, stringField, additionalFields);
        }

        private void CreateAllFieldRecords()
        {
            for (var i = 0; i < 2; i++)
            {
                var id = "Dummy" + (1 + i);
                var entity = NewRecord(FakeConstants.AllFieldTypesType);
                entity.SetField(FakeConstants.AllId, id, this);
                entity.Id = id;
                entity.SetField(FakeConstants.AllString, "All Fields Record" + (1 + i), this);
                entity.SetField(FakeConstants.AllMemo, ("All Fields Record" + (1 + i) + "\n").ReplicateString(5), this);
                entity.SetField(FakeConstants.AllPicklist, new PicklistOption("Key", "Label"), this);
                entity.SetField(FakeConstants.AllPicklistMulti, new[] { new PicklistOption("Key", "Label"), new PicklistOption("Key3", "Label3") }, this);
                entity.SetField(FakeConstants.AllInteger, 1, this);
                entity.SetField(FakeConstants.AllIntegerPicklist, 2, this);
                entity.SetField(FakeConstants.AllBoolean, true, this);
                entity.SetField(FakeConstants.AllBooleanManaged, true, this);
                entity.SetField(FakeConstants.AllDate, DateTime.Today, this);
                entity.SetField(FakeConstants.AllDateIncludeTime, DateTime.Now, this);
                entity.SetField(FakeConstants.AllLookup, this.ToLookupWithAltDisplayNameName(GetMainRecord()), this);
                entity.SetField(FakeConstants.AllLookupMulti, this.ToLookupWithAltDisplayNameName(this.GetFirst(FakeConstants.RecordType2)), this);
                entity.SetField(FakeConstants.AllDecimal, (decimal)2, this);
                entity.SetField(FakeConstants.AllMoney, (decimal)3, this);
                entity.SetField(FakeConstants.AllPassword, new Password("Fake", false, true), this);
                entity.SetField(FakeConstants.AllFolder, new Folder(CoreTest.TestingFolder), this);
                entity.SetField(FakeConstants.AllDouble, (double)4, this);
                entity.SetField(FakeConstants.AllRecordType, new RecordType(FakeConstants.RecordType, FakeConstants.RecordType), this);
                entity.SetField(FakeConstants.AllActivityParty, this.RetrieveAll(FakeConstants.FakeActivityPartyType, null), this);
                Create(entity, null);
            }
        }

        private void Create100DummyRecords()
        {
            for (var i = 0; i < 100; i++)
            {
                var id = "Dummy" + (1 + i);
                var entity = NewRecord(FakeConstants.RecordType);
                entity.SetField(FakeConstants.Id, id, this);
                entity.Id = id;
                entity.SetField(FakeConstants.PrimaryField, "Dummy Record " + (1 + i), this);
                Create(entity, null);
            }
        }

        internal static IEnumerable<One2ManyRelationshipMetadata> GetFakeOne2ManyRelationshipMetadata()
        {
            return new[]
            {
                new One2ManyRelationshipMetadata()
                {
                    ReferencedAttribute = FakeConstants.Id,
                    ReferencedEntity = FakeConstants.RecordType,
                    ReferencingAttribute = FakeConstants.LookupField,
                    ReferencingEntity = FakeConstants.RecordType
                },
                new One2ManyRelationshipMetadata()
                {
                    ReferencedAttribute = FakeConstants.FakeContactTypeId,
                    ReferencedEntity = FakeConstants.FakeContactType,
                    ReferencingAttribute = FakeConstants.FakeActivityPartyTypePartyField,
                    ReferencingEntity = FakeConstants.FakeActivityPartyType
                },
                new One2ManyRelationshipMetadata()
                {
                    ReferencedAttribute = FakeConstants.FakeEmailTypeId,
                    ReferencedEntity = FakeConstants.FakeEmailType,
                    ReferencingAttribute = FakeConstants.FakeActivityPartyTypeActivityField,
                    ReferencingEntity = FakeConstants.FakeActivityPartyType
                }
            };
        }

        internal static IEnumerable<Many2ManyRelationshipMetadata> GetFakeMany2ManyRelationshipMetadata()
        {
            return new[]
            {
                new Many2ManyRelationshipMetadata()
                {
                    RecordType1 = FakeConstants.RecordType,
                    Entity1IntersectAttribute = FakeConstants.IntersectField1,
                    Entity2IntersectAttribute = FakeConstants.IntersectField2,
                    RecordType2 = FakeConstants.RecordType,
                    IntersectEntityName = FakeConstants.IntersectRecordName
                }
            };
        }

        internal static IEnumerable<RecordMetadata> GetFakeRecordMetadata()
        {
            var recordMetadata = new RecordMetadata
            {
                SchemaName = FakeConstants.RecordType,
                Fields = GetFakeRecordFieldMetadata(),
                DisplayName = "Fake Record",
                CollectionName = "Fake Records",
                Views = GetFakeRecordViewMetadata()
            };
            var recordMetadata2 = new RecordMetadata
            {
                SchemaName = FakeConstants.RecordType2,
                Fields = GetFakeRecordFieldMetadata2(),
                DisplayName = "Fake Record 2",
                CollectionName = "Fake Records 2"
            };
            return new[]
            {
                recordMetadata, recordMetadata2, CreateFakePostTypeMetadata(), CreateFakeActivityTypeMetadata(),
                CreateContactTypeMetadata(), CreateActivityPartyTypeMetadata(), CreateAllFieldTypesMetadata()
            };
        }

        private static RecordMetadata CreateAllFieldTypesMetadata()
        {
            return new RecordMetadata
            {
                SchemaName = FakeConstants.AllFieldTypesType,
                DisplayName = FakeConstants.AllFieldTypesTypeLabel,
                CollectionName = FakeConstants.AllFieldTypesTypeLabel,
                Fields = new FieldMetadata[]
                {
                    new UniqueidentifierFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllId, FakeConstants.AllId) { IsPrimaryKey = true },
                    new StringFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllString, FakeConstants.AllString) { IsPrimaryField = true },
                    new MemoFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllMemo, FakeConstants.AllMemo),
                    new PicklistFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllPicklist, FakeConstants.AllPicklist, new PicklistOptionSet(new [] { new PicklistOption("Key", "Value"), new PicklistOption("Key2", "Value2") })),
                    new PicklistFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllPicklistMulti, FakeConstants.AllPicklistMulti, new PicklistOptionSet(new [] { new PicklistOption("Key", "Value"), new PicklistOption("Key2", "Value2"), new PicklistOption("Key3", "Value3") })) { IsMultiSelect = true },
                    new IntegerFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllInteger, FakeConstants.AllInteger),
                    new IntegerFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllIntegerPicklist, FakeConstants.AllIntegerPicklist) { IntegerFormat = IntegerType.Language },
                    new BooleanFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllBoolean, FakeConstants.AllBoolean),
                    new BooleanFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllBooleanManaged, FakeConstants.AllBooleanManaged),
                    new DateFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllDate, FakeConstants.AllDate),
                    new DateFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllDateIncludeTime, FakeConstants.AllDateIncludeTime) { IncludeTime = true },
                    new LookupFieldMetadata(FakeConstants.AllLookup, FakeConstants.AllLookup, FakeConstants.RecordType),
                    new LookupFieldMetadata(FakeConstants.AllLookupMulti, FakeConstants.AllLookupMulti, FakeConstants.RecordType + "," + FakeConstants.RecordType2),
                    new DecimalFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllDecimal, FakeConstants.AllDecimal),
                    new MoneyFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllMoney, FakeConstants.AllMoney),
                    new PasswordFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllPassword, FakeConstants.AllPassword),
                    new FolderFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllFolder, FakeConstants.AllFolder),
                    new DoubleFieldMetadata(FakeConstants.AllFieldTypesType, FakeConstants.AllDouble, FakeConstants.AllDouble),
                    new RecordTypeFieldMetadata(FakeConstants.AllRecordType, FakeConstants.AllRecordType),
                    new ActivityPartyFieldMetadata(FakeConstants.AllActivityParty, FakeConstants.AllActivityParty),
                }
            };
        }

        private static RecordMetadata CreateActivityPartyTypeMetadata()
        {
            return new RecordMetadata
            {
                SchemaName = FakeConstants.FakeActivityPartyType,
                DisplayName = FakeConstants.FakeActivityPartyTypeLabel,
                CollectionName = FakeConstants.FakeActivityPartyTypeLabel,
                Fields = new FieldMetadata[]
                {
                    new StringFieldMetadata(FakeConstants.FakeActivityPartyTypeId, FakeConstants.FakeContactTypeId) { IsPrimaryKey = true },
                    new StringFieldMetadata(FakeConstants.FakeActivityPartyTypePrimaryField,
                        FakeConstants.FakeContactPrimaryField) {IsPrimaryField = true},
                    new LookupFieldMetadata(FakeConstants.FakeActivityPartyTypePartyField,
                        FakeConstants.FakeActivityPartyTypePartyField, FakeConstants.FakeContactType),
                    new LookupFieldMetadata(FakeConstants.FakeActivityPartyTypeActivityField,
                        FakeConstants.FakeActivityPartyTypeActivityField, FakeConstants.FakeEmailType)
                }
            };
        }

        private static RecordMetadata CreateContactTypeMetadata()
        {
            return new RecordMetadata
            {
                SchemaName = FakeConstants.FakeContactType,
                DisplayName = FakeConstants.FakeContactType,
                CollectionName = FakeConstants.FakeContactType,
                IsActivityParticipant = true,
                Fields = new[]
                {
                    new StringFieldMetadata(FakeConstants.FakeContactTypeId, FakeConstants.FakeContactTypeId) { IsPrimaryKey = true },
                    new StringFieldMetadata(FakeConstants.FakeContactPrimaryField, FakeConstants.FakeContactPrimaryField)
                    {
                        IsPrimaryField = true
                    }
                },
            };
        }

        private static RecordMetadata CreateFakeActivityTypeMetadata()
        {
            return new RecordMetadata
            {
                SchemaName = FakeConstants.FakeEmailType,
                DisplayName = FakeConstants.FakeEmailType,
                CollectionName = FakeConstants.FakeEmailType,
                IsActivityType = true,
                Fields = new FieldMetadata[]
                {
                    new StringFieldMetadata(FakeConstants.FakeEmailTypeId, FakeConstants.FakeEmailTypeId) { IsPrimaryKey = true },
                    new StringFieldMetadata(FakeConstants.FakeEmailTypePrimaryField,
                        FakeConstants.FakeEmailTypePrimaryField) {IsPrimaryField = true},
                    new StringFieldMetadata(FakeConstants.FakeEmailTypeContentField,
                        FakeConstants.FakeEmailTypeContentField),
                    new ActivityPartyFieldMetadata(FakeConstants.FakeEmailTypeActivityParty,
                        FakeConstants.FakeEmailTypeActivityParty)
                }
            };
        }

        private static RecordMetadata CreateFakePostTypeMetadata()
        {
            return new RecordMetadata
            {
                SchemaName = FakeConstants.FakePostType,
                DisplayName = FakeConstants.FakePostType,
                CollectionName = FakeConstants.FakePostType,
                Fields = new[]
                {
                    new StringFieldMetadata(FakeConstants.FakePostTypeId, FakeConstants.FakePostTypeId) { IsPrimaryKey = true },
                    new StringFieldMetadata(FakeConstants.FakePostTypePrimaryField,
                        FakeConstants.FakePostTypePrimaryField) {IsPrimaryField = true}
                }
            };
        }

        internal static IEnumerable<ViewMetadata> GetFakeRecordViewMetadata()
        {
            return new[]
            {
                new ViewMetadata(
                    new[]
                    {
                        new ViewField(FakeConstants.PrimaryField, 1, 150),
                        new ViewField(FakeConstants.StringField, 1, 50),
                        new ViewField(FakeConstants.LookupField, 1, 100),
                        new ViewField(FakeConstants.IntegerField, 1, 100)
                    }) {ViewType = ViewType.LookupView},
                new ViewMetadata(
                    new[]
                    {
                        new ViewField(FakeConstants.PrimaryField, 1, 150),
                        new ViewField(FakeConstants.StringField, 1, 100),
                        new ViewField(FakeConstants.IntegerField, 1, 50),
                        new ViewField(FakeConstants.DateOfBirthField, 1, 100)
                    }) {ViewType = ViewType.MainApplicationView}
            };
        }

        internal static IEnumerable<FieldMetadata> GetFakeRecordFieldMetadata()
        {
            var fieldMetadata = new List<FieldMetadata>(new FieldMetadata[]
            {
                new StringFieldMetadata(FakeConstants.Id, "Id") { IsPrimaryKey = true },
                new StringFieldMetadata(FakeConstants.PrimaryField, "Record Name") {IsPrimaryField = true},
                new IntegerFieldMetadata(FakeConstants.IntegerField, "An Integer", 0, 100000),
                new BooleanFieldMetadata(FakeConstants.BooleanField, "An Check"),
                new StringFieldMetadata(FakeConstants.StringField, "An String"),
                new PicklistFieldMetadata(FakeConstants.ComboField, "An Combo", new PicklistOptionSet(new [] { new PicklistOption("1", "Option 1"), new PicklistOption("2", "Option 2") })), 
                new LookupFieldMetadata(FakeConstants.LookupField, "A Lookup", FakeConstants.RecordType),
                new DateFieldMetadata(FakeConstants.DateOfBirthField,
                    "Date of Birth"),
                new StringFieldMetadata(FakeConstants.HtmlField,
                    "Html"),
                    new DateFieldMetadata(FakeConstants.EnumerableField,
                    "Enumerable"),
                    new DecimalFieldMetadata(FakeConstants.RecordType, FakeConstants.DecimalField, "A Decimal"),
                    new DoubleFieldMetadata(FakeConstants.RecordType, FakeConstants.DoubleField, "A Double"),
                    new MoneyFieldMetadata(FakeConstants.RecordType, FakeConstants.MoneyField, "A Money"),
            });
            for (var i = 1; i <= 50; i++)
                fieldMetadata.Add(new StringFieldMetadata(FakeConstants.StringField + i, "An String" + i));
            return fieldMetadata;
        }

        internal static IEnumerable<FieldMetadata> GetFakeRecordFieldMetadata2()
        {
            return new FieldMetadata[]
            {
                new StringFieldMetadata(FakeConstants.Id2, "Id 2") { IsPrimaryKey = true },
                new StringFieldMetadata(FakeConstants.PrimaryField2, "Record Name 2") {IsPrimaryField = true}
            };
        }

        internal IRecord CreateFakeMainRecord()
        {
            var referencedRecord = CreateFakeRecord(FakeConstants.RecordType, "I Lookup Referenced");
            var fields = new Dictionary<string, object>();
            fields.Add(FakeConstants.BooleanField, true);
            fields.Add(FakeConstants.DateOfBirthField, new DateTime(1980, 11, 15));
            fields.Add(FakeConstants.IntegerField, 999);
            fields.Add(FakeConstants.StringField, "A String");
            //fields.Add(FakeConstants.HtmlField, SampleHtml);
            fields.Add(FakeConstants.LookupField,
                new Lookup(referencedRecord.Type, referencedRecord.Id, "Should Never See This"));
            return CreateFakeRecord(FakeConstants.RecordType, FakeConstants.MainRecordName, fields);
        }

        internal void CreatLinkedRecords(IRecord referencedRecord)
        {
            for (var i = 0; i < 50; i++)
            {
                var fields = new Dictionary<string, object>();
                fields.Add(FakeConstants.BooleanField, true);
                fields.Add(FakeConstants.DateOfBirthField, new DateTime(1980, 11, 15));
                fields.Add(FakeConstants.IntegerField, i);
                fields.Add(FakeConstants.StringField, "A String");
                //fields.Add(FakeConstants.EnumerableField, new List<string>() { { "One" }, { "Two" }});
                //fields.Add(FakeConstants.HtmlField, SampleHtml);
                fields.Add(FakeConstants.LookupField,
                    new Lookup(referencedRecord.Type, referencedRecord.Id, "Should Never See This"));
                CreateFakeRecord(FakeConstants.RecordType, "I Linked To Main Record " + i, fields);
            }
        }

        public static string SampleHtml
        {
            get { return FakeConstants.FakeHtml; }
        }

        public override IsValidResponse VerifyConnection()
        {
            return new IsValidResponse();
        }

        private readonly FakeFormService _formService = new FakeFormService();
        public override IFormService GetFormService()
        {
            return _formService;
        }

        public override IEnumerable<PicklistOption> GetPicklistKeyValues(string field, string recordType, string dependentValue, IRecord record)
        {
            if (field == FakeConstants.AllIntegerPicklist)
                return new[] { new PicklistOption("1", "One"), new PicklistOption("2", "Two"), new PicklistOption("3", "Three") };
            return base.GetPicklistKeyValues(field, recordType, dependentValue, record);
        }
    }
}