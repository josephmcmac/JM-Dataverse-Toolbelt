#region

using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

#endregion

namespace JosephM.Xrm.Test
{
    public class FakeOrganizationService : IOrganizationService
    {
        private readonly Exception _exceptionToThrow;

        public FakeOrganizationService(Exception exceptionToThrow)
        {
            _exceptionToThrow = exceptionToThrow;
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public Guid Create(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(string entityName, Guid id)
        {
            throw new NotImplementedException();
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            if (_exceptionToThrow != null)
                throw _exceptionToThrow;
            throw new NotImplementedException();
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            throw new NotImplementedException();
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            throw new NotImplementedException();
        }

        public void Update(Entity entity)
        {
            throw new NotImplementedException();
        }
    }
}