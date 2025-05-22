using JosephM.Core.Service;
using JosephM.Core.Utility;
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
            var allUsers = request.GetUsersToUpdate();

            var filters = allUsers
                .Select(u =>
                {
                    var filter = new Filter();
                    filter.AddCondition(Fields.systemuser_.systemuserid, ConditionType.Equal, u.Id);
                    filter.AddCondition(Fields.role_.roleid, ConditionType.Equal, request.SecurityRoleToRemove.Id);
                    return filter;
                })
                .ToArray();
            var existingSecurityRoleAssociationUserIds = new HashSet<string>(XrmRecordService.RetrieveAllOrClauses(Relationships.systemuser_.systemuserroles_association.EntityName, filters, new[] { Fields.systemuser_.systemuserid })
                .Select(a => a.GetIdField(Fields.systemuser_.systemuserid)));

            var usersWithSecurityRole = allUsers
                .Where(u => existingSecurityRoleAssociationUserIds.Contains(u.Id))
                .ToArray();

            response.CountRoleNotPresent = allUsers.Count() - usersWithSecurityRole.Count();

            var countUpdated = 0;
            var countToRemove = usersWithSecurityRole.Count();
            controller.UpdateProgress(0, countToRemove, "Removing Security Role");
            var estimator = new TaskEstimator(countToRemove);

            foreach(var userWithSecurityRole in usersWithSecurityRole)
            {
                try
                {
                    XrmRecordService.XrmService.Disassociate(Relationships.systemuser_.systemuserroles_association.Name, Fields.systemuser_.systemuserid, new Guid(userWithSecurityRole.Id), Fields.role_.roleid, new Guid(request.SecurityRoleToRemove.Id));
                    response.CountRoleRemoved++;
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new RemoveRolesResponseItem(userWithSecurityRole.Id, userWithSecurityRole.GetStringField(Fields.systemuser_.fullname), ex));
                    response.NumberOfErrors++;
                }
                countUpdated++;
                controller.UpdateProgress(countUpdated, countToRemove, estimator.GetProgressString(countUpdated, taskName: "Removing Security Role"));
            }

            response.Message = "Updates Completed";
        }
    }
}