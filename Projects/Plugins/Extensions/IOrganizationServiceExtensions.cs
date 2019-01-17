using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Crm.CommunitySupport.Extensions
{
    public static class IOrganizationServiceExtensions
    {
        /// <summary>
        /// Retrieve an entity by reference, optionally specifying columns to select.
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="entityReference"></param>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IOrganizationService organizationService, EntityReference entityReference, params string[] columns)
        {
            return organizationService.Retrieve(entityReference.LogicalName, entityReference.Id, columns);
        }

        /// <summary>
        /// Retrieve an entity by reference, optionally specifying columns to select.
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="entityReference"></param>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public static Entity Retrieve(this IOrganizationService organizationService, string entityLogicalName, Guid entityId, params string[] columns)
        {
            var columnSet = !columns.Any() ? new ColumnSet(true) : new ColumnSet(columns);
            return organizationService.Retrieve(entityLogicalName, entityId, columnSet);
        }
    }
}
