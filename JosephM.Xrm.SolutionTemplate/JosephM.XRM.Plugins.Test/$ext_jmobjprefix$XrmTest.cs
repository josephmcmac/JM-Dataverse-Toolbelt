using $ext_safeprojectname$.Plugins.Services;
using $ext_safeprojectname$.Plugins.Rollups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using $ext_safeprojectname$.Plugins.Xrm;
using $ext_safeprojectname$.Plugins.SharePoint;
using $ext_safeprojectname$.Plugins.Localisation;

namespace $safeprojectname$
{
    [TestClass]
    public class $ext_jmobjprefix$XrmTest : XrmTest
    {
        //USE THIS IF NEED TO VERIFY SCRIPTS FOR A PARTICULAR SECURITY ROLE
        //private XrmService _xrmService;
        //public override XrmService XrmService
        //{
        //    get
        //    {
        //        if (_xrmService == null)
        //        {
        //            var xrmConnection = new XrmConfiguration()
        //            {
        //                AuthenticationProviderType = XrmConfiguration.AuthenticationProviderType,
        //                DiscoveryServiceAddress = XrmConfiguration.DiscoveryServiceAddress,
        //                OrganizationUniqueName = XrmConfiguration.OrganizationUniqueName,
        //                Username = "",
        //                Password = ""
        //            };
        //            _xrmService = new XrmService(xrmConnection);
        //        }
        //        return _xrmService;
        //    }
        //}

        protected override IEnumerable<string> EntitiesToDelete
        {
            get
            {
                return new string[0];
            }
        }

        private $ext_jmobjprefix$Settings _settings;
        public $ext_jmobjprefix$Settings $ext_jmobjprefix$Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new $ext_jmobjprefix$Settings(XrmService);
                return _settings;
            }
        }

        private $ext_jmobjprefix$Service _service;
        public $ext_jmobjprefix$Service $ext_jmobjprefix$Service
        {
            get
            {
                if (_service == null)
                    _service = new $ext_jmobjprefix$Service(XrmService, $ext_jmobjprefix$Settings);
                return _service;
            }
        }

        private $ext_jmobjprefix$RollupService _RollupService;
        public $ext_jmobjprefix$RollupService $ext_jmobjprefix$RollupService
        {
            get
            {
                if (_RollupService == null)
                    _RollupService = new $ext_jmobjprefix$RollupService(XrmService);
                return _RollupService;
            }
        }

        private $ext_jmobjprefix$SharepointService _sharePointService;
        public $ext_jmobjprefix$SharepointService $ext_jmobjprefix$SharepointService
        {
            get
            {
                if (_sharePointService == null)
                    _sharePointService = new $ext_jmobjprefix$SharepointService(XrmService, new $ext_jmobjprefix$SharePointSettings(XrmService));
                return _sharePointService;
            }
        }

        private LocalisationService _localisationService;
        public LocalisationService LocalisationService
        {
            get
            {
                if (_localisationService == null)
                    _localisationService = new LocalisationService(new LocalisationSettings());
                return _localisationService;
            }
        }

        /// <summary>
        /// Basic script for verifying configured Rollups - not only uses active filter in scenarios
        /// </summary>
        /// <summary>
        /// Basic script for verifying configured Rollups - not only uses active filter in scenarios
        /// </summary>
        public void VerifyRollupScenarios(string parentType, string childType, string referenceField)
        {
            var Rollups = $ext_jmobjprefix$RollupService
                            .GetRollupsForRolledupType(childType)
                            .Where(a => a.RecordTypeWithRollup == parentType)
                            .ToArray();
            var contactFieldsRollupd = Rollups.Select(a => a.FieldRolledup).ToArray();

            var parent1 = CreateTestRecord(parentType);
            foreach (var rollup in Rollups)
            {
                if (rollup.NullAmount != null)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(rollup.NullAmount, parent1.GetField(rollup.RollupField)));
                }
            }
            var parent2 = CreateTestRecord(parentType);

            //create new child linked to parent and verify the rollup fields populated
            var child1 = new Entity(childType);
            PopulateRollupFields(child1);
            child1.SetLookupField(referenceField, parent1);
            child1 = CreateAndRetrieve(child1);
            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsNotNull(child1.GetField(rollup.FieldRolledup));
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //create second child linked to parent and verify the rollup fields added
            var child2 = new Entity(childType);
            PopulateRollupFields(child2);
            child2.SetLookupField(referenceField, parent1);
            child2 = CreateAndRetrieve(child2);

            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                Assert.IsNotNull(child1.GetField(rollup.FieldRolledup));
                Assert.IsNotNull(child2.GetField(rollup.FieldRolledup));
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //change second child to second parent and verify both parents updated
            child2.SetLookupField(referenceField, parent2);
            child2 = UpdateFieldsAndRetreive(child2, referenceField);

            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                Assert.IsNotNull(child1.GetField(rollup.FieldRolledup));
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }
            parent2 = Refresh(parent2);
            foreach (var rollup in Rollups)
            {
                Assert.IsNotNull(child2.GetField(rollup.FieldRolledup));
                var expectedValue2 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue2, parent2.GetField(rollup.RollupField)));
            }
            //triple each value in child 1 and verify updated in parent
            PopulateRollupFields(child1, multiplier: 3);
            child1 = UpdateFieldsAndRetreive(child1, contactFieldsRollupd);

            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                Assert.IsNotNull(child1.GetField(rollup.FieldRolledup));
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //negate value in child 1 and verify updated in parent
            PopulateRollupFields(child1, multiplier: 2);
            child1 = UpdateFieldsAndRetreive(child1, contactFieldsRollupd);

            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                Assert.IsNotNull(child1.GetField(rollup.FieldRolledup));
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //create child  in account 1 and verify updated
            var child3 = new Entity(childType);
            PopulateRollupFields(child3);
            child3.SetLookupField(referenceField, parent1);
            child3 = CreateAndRetrieve(child3);
            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                Assert.IsNotNull(child3.GetField(rollup.FieldRolledup));
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //deactivate and verify updated
            XrmService.SetState(child3.LogicalName, child3.Id, 1);
            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }
            //activate and verify updated
            XrmService.SetState(child3.LogicalName, child3.Id, 0);
            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //delete and verify updated
            Delete(child3);
            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                Assert.IsNotNull(child1.GetField(rollup.FieldRolledup));
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //lets create one without the fields populated just to verify no error
            child3 = new Entity(childType);
            child3.SetLookupField(referenceField, parent1);
            child3 = CreateAndRetrieve(child3);
            parent1 = Refresh(parent1);
            foreach (var rollup in Rollups)
            {
                var expectedValue1 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent1.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue1, parent1.GetField(rollup.RollupField)));
            }

            //same activate / deactivate / delete for the parent 2
            //this includes changing to not exists

            //deactivate and verify updated
            XrmService.SetState(child2.LogicalName, child2.Id, 1);
            parent2 = Refresh(parent2);
            foreach (var rollup in Rollups)
            {
                var expectedValue2 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent2.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue2, parent2.GetField(rollup.RollupField)));
            }
            //activate and verify updated
            XrmService.SetState(child2.LogicalName, child2.Id, 0);
            parent2 = Refresh(parent2);
            foreach (var rollup in Rollups)
            {
                var expectedValue2 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent2.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue2, parent2.GetField(rollup.RollupField)));
            }

            //delete and verify updated
            Delete(child2);
            parent2 = Refresh(parent2);
            foreach (var rollup in Rollups)
            {
                var expectedValue2 = $ext_jmobjprefix$RollupService.GetRollup(rollup, parent2.Id);
                Assert.IsTrue(XrmEntity.FieldsEqual(expectedValue2, parent2.GetField(rollup.RollupField)));
            }

            DeleteMyToday();
        }

        private void PopulateRollupFields(Entity contact, int multiplier = 1)
        {
            var i = 0;
            foreach (var rollup in $ext_jmobjprefix$RollupService.GetRollupsForRolledupType(contact.LogicalName))
            {
                i++;
                if (rollup.ObjectType == typeof(int)
                    && rollup.RollupType == RollupType.Count
                    && XrmService.GetFieldMetadata(rollup.FieldRolledup, rollup.RecordTypeRolledup).AttributeType == AttributeTypeCode.Lookup)
                {
                    //this is for count of a lookup
                    //for this script it will already be populated
                }
                else if (rollup.ObjectType == typeof(int))
                {
                    contact.SetField(rollup.FieldRolledup, i * multiplier);
                }
                else if (rollup.ObjectType == typeof(Money))
                {
                    contact.SetField(rollup.FieldRolledup, new Money(i * multiplier));
                }
                else if (rollup.ObjectType == typeof(decimal))
                {
                    contact.SetField(rollup.FieldRolledup, new decimal(i * multiplier));
                }
                else if (rollup.ObjectType == typeof(DateTime))
                {
                    contact.SetField(rollup.FieldRolledup, new DateTime(DateTime.Now.Year, i, i * multiplier));
                }
                else if (rollup.ObjectType == typeof(string))
                {
                    contact.SetField(rollup.FieldRolledup, (i * multiplier).ToString());
                }
                else if (rollup.ObjectType == typeof(bool)
                    && rollup.RollupType == RollupType.Exists
                    && XrmService.GetFieldMetadata(rollup.FieldRolledup, rollup.RecordTypeRolledup).AttributeType == AttributeTypeCode.Lookup)
                {
                    //this is for exists
                    //for this script it will already be populated
                }
                else
                {
                    throw new NotImplementedException("Not implemented for type");
                }
            }
        }
    }
}