using System;
using Microsoft.Xrm.Sdk;

namespace $safeprojectname$.Xrm
{
    public class OrganisationSettings
    {
        public XrmService XrmService { get; set; }

        public OrganisationSettings(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        private Entity _organisation;

        public Entity Organisation
        {
            get
            {
                if (_organisation == null)
                    _organisation = XrmService.GetFirst("organization",
                        new[] { "basecurrencyid", "businessclosurecalendarid" });
                return _organisation;
            }
        }

        public Guid BaseCurrencyId
        {
            get
            {
                var value = XrmEntity.GetLookupGuid(Organisation, "basecurrencyid");
                if (!value.HasValue)
                    throw new NullReferenceException(string.Format("Error Getting {0} From The {1} Record",
                        XrmService.GetFieldLabel("basecurrencyid", "organization"),
                        XrmService.GetEntityLabel("organization")));
                return value.Value;
            }
        }

        public Guid BusinessClosureCalendarId
        {
            get
            {
                var value = XrmEntity.GetField(Organisation, "businessclosurecalendarid");
                if (value == null)
                    throw new NullReferenceException(string.Format("Error Getting {0} From The {1} Record",
                        XrmService.GetFieldLabel("businessclosurecalendarid", "organization"),
                        XrmService.GetEntityLabel("organization")));
                return (Guid)value;
            }
        }
    }
}