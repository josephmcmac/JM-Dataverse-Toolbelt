using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.Xrm.Test
{
    public class Entities
    {
        public const string contact = "contact";
        public const string account = "account";
        public const string jmcg_testentitythree = "jmcg_testentitythree";
        public const string jmcg_testentitytwo = "jmcg_testentitytwo";
        public const string jmcg_testentity = "jmcg_testentity";
    }
    public class Relationships
    {
        public class contact_
        {
            public class contactinvoices_association
            {
                public const string Name = "contactinvoices_association";
                public const string EntityName = "contactinvoices";
            }
            public class contactorders_association
            {
                public const string Name = "contactorders_association";
                public const string EntityName = "contactorders";
            }
            public class contactleads_association
            {
                public const string Name = "contactleads_association";
                public const string EntityName = "contactleads";
            }
            public class listcontact_association
            {
                public const string Name = "listcontact_association";
                public const string EntityName = "listmember";
            }
            public class contactquotes_association
            {
                public const string Name = "contactquotes_association";
                public const string EntityName = "contactquotes";
            }
            public class servicecontractcontacts_association
            {
                public const string Name = "servicecontractcontacts_association";
                public const string EntityName = "servicecontractcontacts";
            }
        }
        public class account_
        {
            public class listaccount_association
            {
                public const string Name = "listaccount_association";
                public const string EntityName = "listmember";
            }
            public class accountleads_association
            {
                public const string Name = "accountleads_association";
                public const string EntityName = "accountleads";
            }
            public class jmcg_testentity_account
            {
                public const string Name = "jmcg_testentity_account";
                public const string EntityName = "jmcg_testentity_account";
            }
        }
        public class jmcg_testentitythree_
        {
        }
        public class jmcg_testentitytwo_
        {
        }
        public class jmcg_testentity_
        {
            public class jmcg_testentity_jmcg_testentity
            {
                public const string Name = "jmcg_testentity_jmcg_testentity";
                public const string EntityName = "jmcg_testentity_jmcg_testentity";
            }
            public class jmcg_testentity_account
            {
                public const string Name = "jmcg_testentity_account";
                public const string EntityName = "jmcg_testentity_account";
            }
        }
    }
    public class Fields
    {
        public class contact_
        {
            public const string preferredcontactmethodcodename = "preferredcontactmethodcodename";
            public const string emailaddress3 = "emailaddress3";
            public const string emailaddress2 = "emailaddress2";
            public const string emailaddress1 = "emailaddress1";
            public const string address1_city = "address1_city";
            public const string address1_line1 = "address1_line1";
            public const string educationcodename = "educationcodename";
            public const string address1_longitude = "address1_longitude";
            public const string haschildrencodename = "haschildrencodename";
            public const string address2_freighttermscodename = "address2_freighttermscodename";
            public const string modifiedon = "modifiedon";
            public const string aging90 = "aging90";
            public const string overriddencreatedon = "overriddencreatedon";
            public const string yomilastname = "yomilastname";
            public const string yomifirstname = "yomifirstname";
            public const string statuscodename = "statuscodename";
            public const string donotpostalmail = "donotpostalmail";
            public const string yomifullname = "yomifullname";
            public const string accountid = "accountid";
            public const string address1_addresstypecode = "address1_addresstypecode";
            public const string entityimage_timestamp = "entityimage_timestamp";
            public const string modifiedbyyominame = "modifiedbyyominame";
            public const string aging30 = "aging30";
            public const string donotsendmm = "donotsendmm";
            public const string address2_county = "address2_county";
            public const string creditonhold = "creditonhold";
            public const string transactioncurrencyidname = "transactioncurrencyidname";
            public const string donotbulkpostalmail = "donotbulkpostalmail";
            public const string entityimage_url = "entityimage_url";
            public const string preferredsystemuseridyominame = "preferredsystemuseridyominame";
            public const string address1_shippingmethodcode = "address1_shippingmethodcode";
            public const string paymenttermscode = "paymenttermscode";
            public const string gendercode = "gendercode";
            public const string originatingleadid = "originatingleadid";
            public const string preferredsystemuseridname = "preferredsystemuseridname";
            public const string owningbusinessunit = "owningbusinessunit";
            public const string preferredappointmenttimecode = "preferredappointmenttimecode";
            public const string preferredappointmentdaycodename = "preferredappointmentdaycodename";
            public const string address2_stateorprovince = "address2_stateorprovince";
            public const string participatesinworkflowname = "participatesinworkflowname";
            public const string accountrolecodename = "accountrolecodename";
            public const string mobilephone = "mobilephone";
            public const string parentcustomeridyominame = "parentcustomeridyominame";
            public const string address2_country = "address2_country";
            public const string address2_line2 = "address2_line2";
            public const string aging60_base = "aging60_base";
            public const string assistantphone = "assistantphone";
            public const string parentcontactid = "parentcontactid";
            public const string statuscode = "statuscode";
            public const string parentcontactidyominame = "parentcontactidyominame";
            public const string contactid = "contactid";
            public const string creditlimit = "creditlimit";
            public const string birthdate = "birthdate";
            public const string originatingleadidname = "originatingleadidname";
            public const string address1_utcoffset = "address1_utcoffset";
            public const string managerphone = "managerphone";
            public const string versionnumber = "versionnumber";
            public const string donotsendmarketingmaterialname = "donotsendmarketingmaterialname";
            public const string statecodename = "statecodename";
            public const string telephone1 = "telephone1";
            public const string customertypecode = "customertypecode";
            public const string donotbulkpostalmailname = "donotbulkpostalmailname";
            public const string exchangerate = "exchangerate";
            public const string isprivate = "isprivate";
            public const string telephone3 = "telephone3";
            public const string customersizecode = "customersizecode";
            public const string preferredcontactmethodcode = "preferredcontactmethodcode";
            public const string address1_addresstypecodename = "address1_addresstypecodename";
            public const string address2_latitude = "address2_latitude";
            public const string createdon = "createdon";
            public const string donotbulkemail = "donotbulkemail";
            public const string donotfax = "donotfax";
            public const string familystatuscodename = "familystatuscodename";
            public const string annualincome_base = "annualincome_base";
            public const string aging90_base = "aging90_base";
            public const string address1_composite = "address1_composite";
            public const string firstname = "firstname";
            public const string address2_freighttermscode = "address2_freighttermscode";
            public const string preferredappointmenttimecodename = "preferredappointmenttimecodename";
            public const string address2_postalcode = "address2_postalcode";
            public const string educationcode = "educationcode";
            public const string lastusedincampaign = "lastusedincampaign";
            public const string paymenttermscodename = "paymenttermscodename";
            public const string employeeid = "employeeid";
            public const string owneridyominame = "owneridyominame";
            public const string governmentid = "governmentid";
            public const string isautocreate = "isautocreate";
            public const string isbackofficecustomername = "isbackofficecustomername";
            public const string address2_shippingmethodcodename = "address2_shippingmethodcodename";
            public const string address2_line3 = "address2_line3";
            public const string description = "description";
            public const string modifiedby = "modifiedby";
            public const string entityimage = "entityimage";
            public const string timezoneruleversionnumber = "timezoneruleversionnumber";
            public const string spousesname = "spousesname";
            public const string address1_county = "address1_county";
            public const string shippingmethodcodename = "shippingmethodcodename";
            public const string modifiedonbehalfby = "modifiedonbehalfby";
            public const string donotemail = "donotemail";
            public const string donotphonename = "donotphonename";
            public const string pager = "pager";
            public const string address2_postofficebox = "address2_postofficebox";
            public const string address2_telephone1 = "address2_telephone1";
            public const string address2_telephone2 = "address2_telephone2";
            public const string address2_telephone3 = "address2_telephone3";
            public const string originatingleadidyominame = "originatingleadidyominame";
            public const string aging60 = "aging60";
            public const string address1_addressid = "address1_addressid";
            public const string customersizecodename = "customersizecodename";
            public const string address1_freighttermscode = "address1_freighttermscode";
            public const string createdonbehalfbyname = "createdonbehalfbyname";
            public const string owninguser = "owninguser";
            public const string websiteurl = "websiteurl";
            public const string address2_name = "address2_name";
            public const string middlename = "middlename";
            public const string entityimageid = "entityimageid";
            public const string donotphone = "donotphone";
            public const string parentcustomeridname = "parentcustomeridname";
            public const string owneridname = "owneridname";
            public const string creditonholdname = "creditonholdname";
            public const string accountrolecode = "accountrolecode";
            public const string createdonbehalfbyyominame = "createdonbehalfbyyominame";
            public const string address2_composite = "address2_composite";
            public const string territorycodename = "territorycodename";
            public const string address1_country = "address1_country";
            public const string customertypecodename = "customertypecodename";
            public const string owningteam = "owningteam";
            public const string address1_stateorprovince = "address1_stateorprovince";
            public const string externaluseridentifier = "externaluseridentifier";
            public const string isprivatename = "isprivatename";
            public const string preferredserviceidname = "preferredserviceidname";
            public const string modifiedonbehalfbyname = "modifiedonbehalfbyname";
            public const string preferredequipmentid = "preferredequipmentid";
            public const string address1_line3 = "address1_line3";
            public const string processid = "processid";
            public const string address1_freighttermscodename = "address1_freighttermscodename";
            public const string createdonbehalfby = "createdonbehalfby";
            public const string mastercontactidname = "mastercontactidname";
            public const string jobtitle = "jobtitle";
            public const string nickname = "nickname";
            public const string transactioncurrencyid = "transactioncurrencyid";
            public const string managername = "managername";
            public const string address1_telephone1 = "address1_telephone1";
            public const string address1_telephone2 = "address1_telephone2";
            public const string address1_telephone3 = "address1_telephone3";
            public const string isbackofficecustomer = "isbackofficecustomer";
            public const string suffix = "suffix";
            public const string donotemailname = "donotemailname";
            public const string childrensnames = "childrensnames";
            public const string fax = "fax";
            public const string masterid = "masterid";
            public const string mastercontactidyominame = "mastercontactidyominame";
            public const string assistantname = "assistantname";
            public const string yomimiddlename = "yomimiddlename";
            public const string ownerid = "ownerid";
            public const string address2_utcoffset = "address2_utcoffset";
            public const string creditlimit_base = "creditlimit_base";
            public const string address2_fax = "address2_fax";
            public const string merged = "merged";
            public const string owneridtype = "owneridtype";
            public const string address2_longitude = "address2_longitude";
            public const string defaultpricelevelid = "defaultpricelevelid";
            public const string ftpsiteurl = "ftpsiteurl";
            public const string preferredequipmentidname = "preferredequipmentidname";
            public const string annualincome = "annualincome";
            public const string createdbyname = "createdbyname";
            public const string defaultpricelevelidname = "defaultpricelevelidname";
            public const string address1_shippingmethodcodename = "address1_shippingmethodcodename";
            public const string accountidname = "accountidname";
            public const string address1_primarycontactname = "address1_primarycontactname";
            public const string statecode = "statecode";
            public const string address1_line2 = "address1_line2";
            public const string modifiedonbehalfbyyominame = "modifiedonbehalfbyyominame";
            public const string createdby = "createdby";
            public const string parentcustomerid = "parentcustomerid";
            public const string address2_addresstypecode = "address2_addresstypecode";
            public const string address2_upszone = "address2_upszone";
            public const string donotfaxname = "donotfaxname";
            public const string gendercodename = "gendercodename";
            public const string address2_addresstypecodename = "address2_addresstypecodename";
            public const string salutation = "salutation";
            public const string leadsourcecodename = "leadsourcecodename";
            public const string address1_postalcode = "address1_postalcode";
            public const string stageid = "stageid";
            public const string utcconversiontimezonecode = "utcconversiontimezonecode";
            public const string donotbulkemailname = "donotbulkemailname";
            public const string participatesinworkflow = "participatesinworkflow";
            public const string preferredappointmentdaycode = "preferredappointmentdaycode";
            public const string preferredserviceid = "preferredserviceid";
            public const string address2_addressid = "address2_addressid";
            public const string anniversary = "anniversary";
            public const string importsequencenumber = "importsequencenumber";
            public const string address2_city = "address2_city";
            public const string haschildrencode = "haschildrencode";
            public const string telephone2 = "telephone2";
            public const string mergedname = "mergedname";
            public const string subscriptionid = "subscriptionid";
            public const string familystatuscode = "familystatuscode";
            public const string department = "department";
            public const string preferredsystemuserid = "preferredsystemuserid";
            public const string aging30_base = "aging30_base";
            public const string address1_name = "address1_name";
            public const string address1_fax = "address1_fax";
            public const string address1_latitude = "address1_latitude";
            public const string address2_shippingmethodcode = "address2_shippingmethodcode";
            public const string parentcustomeridtype = "parentcustomeridtype";
            public const string parentcontactidname = "parentcontactidname";
            public const string modifiedbyname = "modifiedbyname";
            public const string createdbyyominame = "createdbyyominame";
            public const string leadsourcecode = "leadsourcecode";
            public const string address2_line1 = "address2_line1";
            public const string address1_upszone = "address1_upszone";
            public const string lastname = "lastname";
            public const string accountidyominame = "accountidyominame";
            public const string shippingmethodcode = "shippingmethodcode";
            public const string territorycode = "territorycode";
            public const string donotpostalmailname = "donotpostalmailname";
            public const string numberofchildren = "numberofchildren";
            public const string address1_postofficebox = "address1_postofficebox";
            public const string address2_primarycontactname = "address2_primarycontactname";
            public const string fullname = "fullname";
        }
        public class account_
        {
            public const string preferredcontactmethodcodename = "preferredcontactmethodcodename";
            public const string emailaddress3 = "emailaddress3";
            public const string emailaddress2 = "emailaddress2";
            public const string emailaddress1 = "emailaddress1";
            public const string masteraccountidyominame = "masteraccountidyominame";
            public const string address1_city = "address1_city";
            public const string address1_line1 = "address1_line1";
            public const string address2_freighttermscodename = "address2_freighttermscodename";
            public const string modifiedon = "modifiedon";
            public const string aging90 = "aging90";
            public const string overriddencreatedon = "overriddencreatedon";
            public const string websiteurl = "websiteurl";
            public const string address1_longitude = "address1_longitude";
            public const string donotpostalmail = "donotpostalmail";
            public const string accountid = "accountid";
            public const string address1_addresstypecode = "address1_addresstypecode";
            public const string entityimage_timestamp = "entityimage_timestamp";
            public const string sharesoutstanding = "sharesoutstanding";
            public const string donotsendmm = "donotsendmm";
            public const string primarycontactidname = "primarycontactidname";
            public const string creditonhold = "creditonhold";
            public const string transactioncurrencyidname = "transactioncurrencyidname";
            public const string aging30 = "aging30";
            public const string donotbulkpostalmail = "donotbulkpostalmail";
            public const string preferredsystemuseridyominame = "preferredsystemuseridyominame";
            public const string address1_shippingmethodcode = "address1_shippingmethodcode";
            public const string paymenttermscode = "paymenttermscode";
            public const string businesstypecodename = "businesstypecodename";
            public const string originatingleadid = "originatingleadid";
            public const string masteraccountidname = "masteraccountidname";
            public const string preferredsystemuseridname = "preferredsystemuseridname";
            public const string accountcategorycode = "accountcategorycode";
            public const string preferredappointmentdaycodename = "preferredappointmentdaycodename";
            public const string address2_stateorprovince = "address2_stateorprovince";
            public const string participatesinworkflowname = "participatesinworkflowname";
            public const string territoryid = "territoryid";
            public const string address2_country = "address2_country";
            public const string accountcategorycodename = "accountcategorycodename";
            public const string address2_line2 = "address2_line2";
            public const string aging60_base = "aging60_base";
            public const string address1_line3 = "address1_line3";
            public const string address1_freighttermscode = "address1_freighttermscode";
            public const string creditlimit = "creditlimit";
            public const string parentaccountidname = "parentaccountidname";
            public const string originatingleadidname = "originatingleadidname";
            public const string address1_utcoffset = "address1_utcoffset";
            public const string preferredappointmenttimecode = "preferredappointmenttimecode";
            public const string donotsendmarketingmaterialname = "donotsendmarketingmaterialname";
            public const string numberofemployees = "numberofemployees";
            public const string name = "name";
            public const string statecodename = "statecodename";
            public const string accountclassificationcode = "accountclassificationcode";
            public const string revenue = "revenue";
            public const string customertypecode = "customertypecode";
            public const string donotbulkpostalmailname = "donotbulkpostalmailname";
            public const string exchangerate = "exchangerate";
            public const string address2_county = "address2_county";
            public const string isprivate = "isprivate";
            public const string primarycontactid = "primarycontactid";
            public const string telephone3 = "telephone3";
            public const string parentaccountid = "parentaccountid";
            public const string address2_city = "address2_city";
            public const string statuscode = "statuscode";
            public const string address1_addresstypecodename = "address1_addresstypecodename";
            public const string address2_latitude = "address2_latitude";
            public const string createdon = "createdon";
            public const string donotbulkemail = "donotbulkemail";
            public const string address2_line1 = "address2_line1";
            public const string donotfax = "donotfax";
            public const string marketcap = "marketcap";
            public const string address1_composite = "address1_composite";
            public const string ownershipcode = "ownershipcode";
            public const string statuscodename = "statuscodename";
            public const string owningbusinessunit = "owningbusinessunit";
            public const string preferredappointmenttimecodename = "preferredappointmenttimecodename";
            public const string address2_postalcode = "address2_postalcode";
            public const string lastusedincampaign = "lastusedincampaign";
            public const string paymenttermscodename = "paymenttermscodename";
            public const string utcconversiontimezonecode = "utcconversiontimezonecode";
            public const string owneridyominame = "owneridyominame";
            public const string entityimage_url = "entityimage_url";
            public const string address2_shippingmethodcodename = "address2_shippingmethodcodename";
            public const string address2_line3 = "address2_line3";
            public const string description = "description";
            public const string modifiedby = "modifiedby";
            public const string timezoneruleversionnumber = "timezoneruleversionnumber";
            public const string address1_county = "address1_county";
            public const string createdbyname = "createdbyname";
            public const string shippingmethodcodename = "shippingmethodcodename";
            public const string preferredcontactmethodcode = "preferredcontactmethodcode";
            public const string modifiedonbehalfby = "modifiedonbehalfby";
            public const string donotemail = "donotemail";
            public const string territorycode = "territorycode";
            public const string donotphonename = "donotphonename";
            public const string address2_postofficebox = "address2_postofficebox";
            public const string address2_telephone1 = "address2_telephone1";
            public const string address2_telephone2 = "address2_telephone2";
            public const string address2_telephone3 = "address2_telephone3";
            public const string originatingleadidyominame = "originatingleadidyominame";
            public const string address1_addressid = "address1_addressid";
            public const string territoryidname = "territoryidname";
            public const string creditlimit_base = "creditlimit_base";
            public const string yominame = "yominame";
            public const string createdonbehalfbyname = "createdonbehalfbyname";
            public const string owninguser = "owninguser";
            public const string industrycode = "industrycode";
            public const string address2_name = "address2_name";
            public const string donotpostalmailname = "donotpostalmailname";
            public const string owneridtype = "owneridtype";
            public const string entityimageid = "entityimageid";
            public const string aging60 = "aging60";
            public const string territorycodename = "territorycodename";
            public const string businesstypecode = "businesstypecode";
            public const string owneridname = "owneridname";
            public const string entityimage = "entityimage";
            public const string modifiedonbehalfbyname = "modifiedonbehalfbyname";
            public const string createdonbehalfbyyominame = "createdonbehalfbyyominame";
            public const string address2_composite = "address2_composite";
            public const string accountratingcodename = "accountratingcodename";
            public const string shippingmethodcode = "shippingmethodcode";
            public const string address1_country = "address1_country";
            public const string customertypecodename = "customertypecodename";
            public const string owningteam = "owningteam";
            public const string address1_stateorprovince = "address1_stateorprovince";
            public const string isprivatename = "isprivatename";
            public const string creditonholdname = "creditonholdname";
            public const string preferredequipmentid = "preferredequipmentid";
            public const string processid = "processid";
            public const string address1_freighttermscodename = "address1_freighttermscodename";
            public const string createdonbehalfby = "createdonbehalfby";
            public const string transactioncurrencyid = "transactioncurrencyid";
            public const string accountratingcode = "accountratingcode";
            public const string address1_telephone1 = "address1_telephone1";
            public const string address1_telephone2 = "address1_telephone2";
            public const string address1_telephone3 = "address1_telephone3";
            public const string address1_postofficebox = "address1_postofficebox";
            public const string customersizecodename = "customersizecodename";
            public const string donotemailname = "donotemailname";
            public const string defaultpricelevelidname = "defaultpricelevelidname";
            public const string fax = "fax";
            public const string masterid = "masterid";
            public const string sic = "sic";
            public const string ownerid = "ownerid";
            public const string address2_utcoffset = "address2_utcoffset";
            public const string stageid = "stageid";
            public const string accountnumber = "accountnumber";
            public const string address2_fax = "address2_fax";
            public const string revenue_base = "revenue_base";
            public const string merged = "merged";
            public const string address2_longitude = "address2_longitude";
            public const string industrycodename = "industrycodename";
            public const string defaultpricelevelid = "defaultpricelevelid";
            public const string preferredequipmentidname = "preferredequipmentidname";
            public const string aging90_base = "aging90_base";
            public const string donotphone = "donotphone";
            public const string address1_shippingmethodcodename = "address1_shippingmethodcodename";
            public const string address1_primarycontactname = "address1_primarycontactname";
            public const string modifiedbyyominame = "modifiedbyyominame";
            public const string address1_line2 = "address1_line2";
            public const string modifiedonbehalfbyyominame = "modifiedonbehalfbyyominame";
            public const string createdby = "createdby";
            public const string address2_addresstypecode = "address2_addresstypecode";
            public const string address2_upszone = "address2_upszone";
            public const string donotfaxname = "donotfaxname";
            public const string marketcap_base = "marketcap_base";
            public const string address2_addresstypecodename = "address2_addresstypecodename";
            public const string ownershipcodename = "ownershipcodename";
            public const string address1_postalcode = "address1_postalcode";
            public const string tickersymbol = "tickersymbol";
            public const string customersizecode = "customersizecode";
            public const string preferredserviceidname = "preferredserviceidname";
            public const string donotbulkemailname = "donotbulkemailname";
            public const string participatesinworkflow = "participatesinworkflow";
            public const string stockexchange = "stockexchange";
            public const string preferredserviceid = "preferredserviceid";
            public const string importsequencenumber = "importsequencenumber";
            public const string telephone2 = "telephone2";
            public const string mergedname = "mergedname";
            public const string versionnumber = "versionnumber";
            public const string preferredsystemuserid = "preferredsystemuserid";
            public const string telephone1 = "telephone1";
            public const string aging30_base = "aging30_base";
            public const string address1_name = "address1_name";
            public const string address1_fax = "address1_fax";
            public const string address1_latitude = "address1_latitude";
            public const string address2_shippingmethodcode = "address2_shippingmethodcode";
            public const string primarycontactidyominame = "primarycontactidyominame";
            public const string accountclassificationcodename = "accountclassificationcodename";
            public const string preferredappointmentdaycode = "preferredappointmentdaycode";
            public const string modifiedbyname = "modifiedbyname";
            public const string createdbyyominame = "createdbyyominame";
            public const string address2_freighttermscode = "address2_freighttermscode";
            public const string address1_upszone = "address1_upszone";
            public const string address2_addressid = "address2_addressid";
            public const string ftpsiteurl = "ftpsiteurl";
            public const string parentaccountidyominame = "parentaccountidyominame";
            public const string address2_primarycontactname = "address2_primarycontactname";
            public const string statecode = "statecode";
        }
        public class jmcg_testentitythree_
        {
            public const string createdonbehalfbyyominame = "createdonbehalfbyyominame";
            public const string owninguser = "owninguser";
            public const string owningteam = "owningteam";
            public const string statecode = "statecode";
            public const string owneridname = "owneridname";
            public const string statecodename = "statecodename";
            public const string createdonbehalfby = "createdonbehalfby";
            public const string importsequencenumber = "importsequencenumber";
            public const string jmcg_name = "jmcg_name";
            public const string modifiedbyyominame = "modifiedbyyominame";
            public const string jmcg_testentitythreeid = "jmcg_testentitythreeid";
            public const string utcconversiontimezonecode = "utcconversiontimezonecode";
            public const string createdbyyominame = "createdbyyominame";
            public const string owningbusinessunit = "owningbusinessunit";
            public const string modifiedbyname = "modifiedbyname";
            public const string versionnumber = "versionnumber";
            public const string modifiedby = "modifiedby";
            public const string createdby = "createdby";
            public const string timezoneruleversionnumber = "timezoneruleversionnumber";
            public const string owneridtype = "owneridtype";
            public const string statuscodename = "statuscodename";
            public const string owneridyominame = "owneridyominame";
            public const string modifiedon = "modifiedon";
            public const string modifiedonbehalfbyyominame = "modifiedonbehalfbyyominame";
            public const string statuscode = "statuscode";
            public const string createdbyname = "createdbyname";
            public const string createdon = "createdon";
            public const string createdonbehalfbyname = "createdonbehalfbyname";
            public const string modifiedonbehalfbyname = "modifiedonbehalfbyname";
            public const string modifiedonbehalfby = "modifiedonbehalfby";
            public const string ownerid = "ownerid";
            public const string overriddencreatedon = "overriddencreatedon";
        }
        public class jmcg_testentitytwo_
        {
            public const string createdonbehalfbyyominame = "createdonbehalfbyyominame";
            public const string owninguser = "owninguser";
            public const string statecode = "statecode";
            public const string owneridname = "owneridname";
            public const string statuscode = "statuscode";
            public const string createdonbehalfby = "createdonbehalfby";
            public const string importsequencenumber = "importsequencenumber";
            public const string jmcg_name = "jmcg_name";
            public const string jmcg_testentitytwoid = "jmcg_testentitytwoid";
            public const string utcconversiontimezonecode = "utcconversiontimezonecode";
            public const string createdbyyominame = "createdbyyominame";
            public const string owningbusinessunit = "owningbusinessunit";
            public const string modifiedbyname = "modifiedbyname";
            public const string owningteam = "owningteam";
            public const string modifiedby = "modifiedby";
            public const string modifiedbyyominame = "modifiedbyyominame";
            public const string createdby = "createdby";
            public const string timezoneruleversionnumber = "timezoneruleversionnumber";
            public const string owneridtype = "owneridtype";
            public const string statuscodename = "statuscodename";
            public const string owneridyominame = "owneridyominame";
            public const string modifiedon = "modifiedon";
            public const string modifiedonbehalfbyyominame = "modifiedonbehalfbyyominame";
            public const string statecodename = "statecodename";
            public const string createdbyname = "createdbyname";
            public const string createdon = "createdon";
            public const string createdonbehalfbyname = "createdonbehalfbyname";
            public const string modifiedonbehalfbyname = "modifiedonbehalfbyname";
            public const string versionnumber = "versionnumber";
            public const string modifiedonbehalfby = "modifiedonbehalfby";
            public const string ownerid = "ownerid";
            public const string overriddencreatedon = "overriddencreatedon";
        }
        public class jmcg_testentity_
        {
            public const string createdonbehalfbyyominame = "createdonbehalfbyyominame";
            public const string modifiedonbehalfby = "modifiedonbehalfby";
            public const string transactioncurrencyidname = "transactioncurrencyidname";
            public const string jmcg_decimal = "jmcg_decimal";
            public const string owneridname = "owneridname";
            public const string statecodename = "statecodename";
            public const string owninguser = "owninguser";
            public const string jmcg_money = "jmcg_money";
            public const string createdonbehalfby = "createdonbehalfby";
            public const string transactioncurrencyid = "transactioncurrencyid";
            public const string jmcg_account = "jmcg_account";
            public const string jmcg_boolean = "jmcg_boolean";
            public const string importsequencenumber = "importsequencenumber";
            public const string jmcg_name = "jmcg_name";
            public const string jmcg_stringmultiline = "jmcg_stringmultiline";
            public const string utcconversiontimezonecode = "utcconversiontimezonecode";
            public const string statuscode = "statuscode";
            public const string jmcg_testentityid = "jmcg_testentityid";
            public const string owningbusinessunit = "owningbusinessunit";
            public const string jmcg_string = "jmcg_string";
            public const string owningteam = "owningteam";
            public const string modifiedby = "modifiedby";
            public const string modifiedbyyominame = "modifiedbyyominame";
            public const string timezoneruleversionnumber = "timezoneruleversionnumber";
            public const string jmcg_picklist = "jmcg_picklist";
            public const string owneridtype = "owneridtype";
            public const string statuscodename = "statuscodename";
            public const string createdbyyominame = "createdbyyominame";
            public const string jmcg_integer = "jmcg_integer";
            public const string modifiedbyname = "modifiedbyname";
            public const string jmcg_date = "jmcg_date";
            public const string owneridyominame = "owneridyominame";
            public const string modifiedon = "modifiedon";
            public const string jmcg_accountyominame = "jmcg_accountyominame";
            public const string exchangerate = "exchangerate";
            public const string statecode = "statecode";
            public const string modifiedonbehalfbyyominame = "modifiedonbehalfbyyominame";
            public const string jmcg_booleanname = "jmcg_booleanname";
            public const string createdbyname = "createdbyname";
            public const string createdon = "createdon";
            public const string createdonbehalfbyname = "createdonbehalfbyname";
            public const string createdby = "createdby";
            public const string jmcg_money_base = "jmcg_money_base";
            public const string modifiedonbehalfbyname = "modifiedonbehalfbyname";
            public const string jmcg_picklistname = "jmcg_picklistname";
            public const string versionnumber = "versionnumber";
            public const string jmcg_float = "jmcg_float";
            public const string jmcg_accountname = "jmcg_accountname";
            public const string ownerid = "ownerid";
            public const string overriddencreatedon = "overriddencreatedon";
        }
    }
}
