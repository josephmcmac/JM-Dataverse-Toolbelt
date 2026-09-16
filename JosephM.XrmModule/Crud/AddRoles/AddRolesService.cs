using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XrmModule.Crud.AddRoles
{
    public class AddRolesService :
        ServiceBase<AddRolesRequest, AddRolesResponse, AddRolesResponseItem>
    {
        public XrmRecordService XrmRecordService { get; set; }
        public AddRolesService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public override void ExecuteExtention(AddRolesRequest request, AddRolesResponse response,
            ServiceRequestController controller)
        {
            var allPrincipals = request.GetPrincipalsToUpdate();
            var distinctPrincipalTypes = allPrincipals.Select(p => p.Type).Distinct().ToArray();
            if(distinctPrincipalTypes.Count() != 1)
            {
                throw new Exception($"The principals to process must all be {Entities.systemuser} or {Entities.team} records. There cannot be a mix of both");
            }
            var recordType = distinctPrincipalTypes[0];
            var recordTypePrimaryKey = XrmRecordService.GetPrimaryKey(recordType);
            var recordTypePrimaryField = XrmRecordService.GetPrimaryField(recordType);
            var relationshipEntityName = recordType == Entities.team
                ? Relationships.team_.teamroles_association.EntityName
                : Relationships.systemuser_.systemuserroles_association.EntityName;
            var relationshipName = recordType == Entities.team
                ? Relationships.team_.teamroles_association.Name
                : Relationships.systemuser_.systemuserroles_association.Name;
            var filters = allPrincipals
                .Select(p =>
                {
                    var filter = new Filter();
                    filter.AddCondition(recordTypePrimaryKey, ConditionType.Equal, p.Id);
                    filter.AddCondition(Fields.role_.roleid, ConditionType.Equal, request.SecurityRoleToAdd.Id);
                    return filter;
                })
                .ToArray();
            var existingSecurityRoleAssociationPrincipalIds = new HashSet<string>(XrmRecordService.RetrieveAllOrClauses(relationshipEntityName, filters, new[] { recordTypePrimaryKey })
                .Select(a => a.GetIdField(recordTypePrimaryKey)));
            var principalsWithoutSecurityRole = allPrincipals
                .Where(p => !existingSecurityRoleAssociationPrincipalIds.Contains(p.Id))
                .ToArray();
            response.CountRoleAlreadyPresent = principalsWithoutSecurityRole.Count();

            var countUpdated = 0;
            var countToAdd = principalsWithoutSecurityRole.Count();
            controller.UpdateProgress(0, countToAdd, "Adding Security Role");
            var estimator = new TaskEstimator(countToAdd);

            foreach(var principalWithSecurityRole in principalsWithoutSecurityRole)
            {
                try
                {
                    XrmRecordService.XrmService.Associate(relationshipName, recordTypePrimaryKey, new Guid(principalWithSecurityRole.Id), Fields.role_.roleid, new Guid(request.SecurityRoleToAdd.Id));
                    response.CountRoleAdded++;
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new AddRolesResponseItem(principalWithSecurityRole.Id, principalWithSecurityRole.GetStringField(recordTypePrimaryField), ex));
                    response.NumberOfErrors++;
                }
                countUpdated++;
                controller.UpdateProgress(countUpdated, countToAdd, estimator.GetProgressString(countUpdated, taskName: "Adding Security Role"));
            }

            response.Message = "Updates Completed";
        }
    }
}