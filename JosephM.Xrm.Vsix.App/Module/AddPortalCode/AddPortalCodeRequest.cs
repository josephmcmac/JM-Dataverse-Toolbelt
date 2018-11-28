using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    [Instruction("This process will export html, javascript and css configured in portal records into files in the visual studio project\n\nOnce exported the code may be deployed into the dynamics instance by right clicking the file, then selecting the 'Deploy Into Record' option")]
    public class AddPortalCodeRequest : ServiceRequestBase
    {
        [ReadOnlyWhenSet]
        [DisplayOrder(10)]
        public string ProjectName { get; set; }

        [DisplayOrder(20)]
        [DoNotAllowAdd]
        [RequiredProperty]
        [ReferencedType(Entities.adx_website)]
        [UsePicklist]
        [InitialiseIfOneOption]
        public Lookup WebSite { get; set; }
    }
}