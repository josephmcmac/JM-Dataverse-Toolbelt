using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.XrmModule.Crud.RemoveRoles
{
    public class RemoveRolesService :
        ServiceBase<RemoveRolesRequest, RemoveRolesResponse, RemoveRolesResponseItem>
    {
        public XrmRecordService XrmRecordService { get; set; }
        public RemoveRolesService(XrmRecordService xrmRecordService)
        {
            XrmRecordService = xrmRecordService;
        }

        public override void ExecuteExtention(RemoveRolesRequest request, RemoveRolesResponse response,
            ServiceRequestController controller)
        {
            var allPrincipals = request.GetPrincipalsToUpdate();
            var distinctPrincipalTypes = allPrincipals.Select(p => p.Type).Distinct().ToArray();
            if (distinctPrincipalTypes.Count() != 1)
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
                    filter.AddCondition(Fields.role_.roleid, ConditionType.Equal, request.SecurityRoleToRemove.Id);
                    return filter;
                })
                .ToArray();
            var existingSecurityRoleAssociationUserIds = new HashSet<string>(XrmRecordService.RetrieveAllOrClauses(relationshipEntityName, filters, new[] { recordTypePrimaryKey })
                .Select(a => a.GetIdField(recordTypePrimaryKey)));
            var principalsWithSecurityRole = allPrincipals
                .Where(p => existingSecurityRoleAssociationUserIds.Contains(p.Id))
                .ToArray();
            response.CountRoleNotPresent = allPrincipals.Count() - principalsWithSecurityRole.Count();

            var countUpdated = 0;
            var countToRemove = principalsWithSecurityRole.Count();
            controller.UpdateProgress(0, countToRemove, "Removing Security Role");
            var estimator = new TaskEstimator(countToRemove);

            foreach(var principalWithSecurityRole in principalsWithSecurityRole)
            {
                try
                {
                    XrmRecordService.XrmService.Disassociate(relationshipName, recordTypePrimaryKey, new Guid(principalWithSecurityRole.Id), Fields.role_.roleid, new Guid(request.SecurityRoleToRemove.Id));
                    response.CountRoleRemoved++;
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new RemoveRolesResponseItem(principalWithSecurityRole.Id, principalWithSecurityRole.GetStringField(recordTypePrimaryField), ex));
                    response.NumberOfErrors++;
                }
                countUpdated++;
                controller.UpdateProgress(countUpdated, countToRemove, estimator.GetProgressString(countUpdated, taskName: "Removing Security Role"));
            }

            response.Message = "Updates Completed";
        }
    }
}