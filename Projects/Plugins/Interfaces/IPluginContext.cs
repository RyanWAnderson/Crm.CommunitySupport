using System;
using Microsoft.Xrm.Sdk;
using Crm.CommunitySupport.Extensions;

namespace Crm.CommunitySupport.Plugins {

    public interface IPluginContext {
        /// <summary>
        /// Create a new OrganizationService under the given user context.
        /// </summary>
        /// <param name="userId">A null value indicates the SYSTEM user, Guid.Empty indicates the user context of the given pipeline. Any other value indicates a specific user id.</param>
        IOrganizationService CreateOrganizationService(Guid? userId = null);

        /// <summary>
        /// Get the Target of the execution context
        /// </summary>
        Entity Target { get; }

        /// <summary>
        /// Get the TargetReference of the execution context
        /// </summary>
        EntityReference TargetReference { get; }

        /// <summary>
        /// Get the TargetEntity, with unchanged values from the specified PreImage
        /// </summary>
        Entity GetPrimaryEntity(string preImageName = "");
        TEntity GetPrimaryEntity<TEntity>(string preImageName = "") where TEntity : Entity;

        /// <summary>
        /// Get an immutable PreImage by name
        /// </summary>
        Entity GetPreImage(string name);
        TEntity GetPreImage<TEntity>(string name) where TEntity : Entity;

        /// <summary>
        /// Get an immutable PostImage by name
        /// </summary>
        Entity GetPostImage(string name);
        TEntity GetPostImage<TEntity>(string name) where TEntity : Entity;

        /// <summary>
        /// Trace a formatted message, per the ITracingService implementation
        /// </summary>
        void Trace(string format, params object[] args);
    }
}