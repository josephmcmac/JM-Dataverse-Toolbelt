using JosephM.Core.Service;
using JosephM.Core.Utility;
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
            var allUsers = request.GetUsersToUpdate();

            var filters = allUsers
                .Select(u =>
                {
                    var filter = new Filter();
                    filter.AddCondition(Fields.systemuser_.systemuserid, ConditionType.Equal, u.Id);
                    filter.AddCondition(Fields.role_.roleid, ConditionType.Equal, request.SecurityRoleToAdd.Id);
                    return filter;
                })
                .ToArray();
            var existingSecurityRoleAssociationUserIds = new HashSet<string>(XrmRecordService.RetrieveAllOrClauses(Relationships.systemuser_.systemuserroles_association.EntityName, filters, new[] { Fields.systemuser_.systemuserid })
                .Select(a => a.GetIdField(Fields.systemuser_.systemuserid)));

            var usersWithoutSecurityRole = allUsers
                .Where(u => !existingSecurityRoleAssociationUserIds.Contains(u.Id))
                .ToArray();

            response.CountRoleAlreadyPresent = usersWithoutSecurityRole.Count();

            var countUpdated = 0;
            var countToAdd = usersWithoutSecurityRole.Count();
            controller.UpdateProgress(0, countToAdd, "Adding Security Role");
            var estimator = new TaskEstimator(countToAdd);

            foreach(var userWithSecurityRole in usersWithoutSecurityRole)
            {
                try
                {
                    XrmRecordService.XrmService.Associate(Relationships.systemuser_.systemuserroles_association.Name, Fields.systemuser_.systemuserid, new Guid(userWithSecurityRole.Id), Fields.role_.roleid, new Guid(request.SecurityRoleToAdd.Id));
                    response.CountRoleAdded++;
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new AddRolesResponseItem(userWithSecurityRole.Id, userWithSecurityRole.GetStringField(Fields.systemuser_.fullname), ex));
                    response.NumberOfErrors++;
                }
                countUpdated++;
                controller.UpdateProgress(countUpdated, countToAdd, estimator.GetProgressString(countUpdated, taskName: "Adding Security Role"));
            }

            response.Message = "Updates Completed";
        }
    }
}