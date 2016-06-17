using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Crm.CommunitySupport.Extensions;

namespace Crm.CommunitySupport.Plugins {
    public enum PluginStage {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40,
    }
    public enum PluginMode {
        Synchronous = 0,
        Sync = 0,
        Asynchronous = 1,
        Async = 1,
    }

    /// <summary>
    /// PluginContext helper class to provide methods and properties for rapid development
    /// </summary>
    public partial class PluginContext : IPluginContext {
        #region Constructor(s)
        public PluginContext(IServiceProvider serviceProvider) {
            // Set private members
            _tsTracingService = new TimestampedTracingService(serviceProvider);
            _organizationServiceFactory = serviceProvider.GetService<IOrganizationServiceFactory>();

            // Set public members
            this.XrmContext = serviceProvider.GetService<IPluginExecutionContext>();
            this.Stage = (PluginStage)XrmContext.Stage;
            this.Mode = (PluginMode)XrmContext.Mode;
        }
        #endregion

        /// <summary>
        /// Create a new OrganizationService under the given user context.
        /// </summary>
        /// <param name="userId">A null value indicates the SYSTEM user, Guid.Empty indicates the user context of the given pipeline. Any other value indicates a specific user id.</param>
        /// <returns></returns>
        public IOrganizationService CreateOrganizationService(Guid? userId = null) {
            return _organizationServiceFactory.CreateOrganizationService(userId);
        }
        /// <summary>
        /// Lazyily-initialized IOrganizationService for the User in the current security context
        /// </summary>
        public IOrganizationService DefaultOrganizationService {
            get {
                if (_defulatOrganizationService == null) {
                    _defulatOrganizationService = CreateOrganizationService(XrmContext.InitiatingUserId);
                }
                return _defulatOrganizationService;
            }
            private set {
                _defulatOrganizationService = value;
            }
        }
        /// Lazyily-initialized OrganizationServiceContext based off DefaultOrganizationSerivce
        public OrganizationServiceContext DefaultOrganizationServiceContext {
            get {
                if (_defulatOrganizationServiceContext == null) {
                    _defulatOrganizationServiceContext = new OrganizationServiceContext(DefaultOrganizationService);
                }
                return _defulatOrganizationServiceContext;
            }
            set {
                _defulatOrganizationServiceContext = value;
            }
        }

        /// Get the Target of the execution context
        public Entity Target {
            get {
                return XrmContext.InputParameters.GetItemIfPresent("Target") as Entity;
            }
        }

        /// <summary>
        /// Get the strongly-typed Target of the execution context
        /// </summary>
        public TEntity GetTarget<TEntity>() where TEntity : Entity {
            return Target?.ToEntity<TEntity>();
        }

        /// <summary>
        /// Get the TargetReference of the execution context
        /// </summary>
        public EntityReference TargetReference {
            get {
                return XrmContext.InputParameters.GetItemIfPresent("Target") as EntityReference;
            }
        }

        /// <summary>
        ///  Get a clone of a PreImage
        /// </summary>
        public Entity GetPreImage(string name) {
            return XrmContext.PreEntityImages.GetItemIfPresent(name)?.Clone();
        }

        /// Get a strongly-typed clone of a PreImage
        public TEntity GetPreImage<TEntity>(string name) where TEntity : Entity {
            return GetPreImage(name)?.ToEntity<TEntity>();
        }

        /// Get a clone of a PostImage
        public Entity GetPostImage(string name) {
            return XrmContext.PostEntityImages.GetItemIfPresent(name);
        }

        /// Get a strongly-typed clone of a PostImage
        public TEntity GetPostImage<TEntity>(string name) where TEntity : Entity {
            return GetPostImage(name)?.ToEntity<TEntity>();
        }

        /// Trace a formatted message, per the implemented ITracingService
        public void Trace(string format, params object[] args) {
            try {
                _tracingService.Trace(format, args);
            }
            catch { }
        }

        public Entity GetPrimaryEntity(string preImageName = "") {
            Entity preImage = null, target, result;

            target = this.Target;
            preImage = this.GetPreImage(preImageName);

            Entity delta = target.GetDeltaFrom(preImage);

            if (preImage != null) {
                result = preImage;

            } else {
                EntityReference entRef = this.TargetReference;
                if (entRef == null) {
                    entRef = target?.ToEntityReference();
                }

                if (entRef == null) {
                    throw new NotImplementedException("GetEntity(): Unable to determine the primary Entity");
                }

                IOrganizationService organizationService = this.CreateOrganizationService();
                result = organizationService.Retrieve(
                    entRef.LogicalName, entRef.Id,
                    new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            }

            if (target != null) {
                result.ApplyDelta(target);
            }

            return result;
        }

        public TEntity GetPrimaryEntity<TEntity>(string preImageName = "") where TEntity : Entity {
            return this.GetPrimaryEntity(preImageName).ToEntity<TEntity>();
        }

        #region Fields
        /// <summary>
        /// The underlying Microsoft.Xrm.Sdk.IPluginExecutionContext
        /// </summary>
        public readonly IPluginExecutionContext XrmContext;

        /// <summary>
        /// Strongly typed varient of IPluginExecutionContext.Stage
        /// </summary>
        public readonly PluginStage Stage;

        /// <summary>
        /// Strongly typed varient of IPluginExecutionContext.Mode
        /// </summary>
        public readonly PluginMode Mode;

        /// <summary>
        /// The Microsoft.Xrm.Sdk.ITracingService used and exposed by the PluginContext
        /// </summary>
        private readonly TimestampedTracingService _tsTracingService;
        private ITracingService _tracingService {
            get {
                return _tsTracingService;
            }
        }

        /// <summary>
        /// The Microsoft.Xrm.Sdk.IOrganizationServiceFactory used to create new IOrganizationSerivces
        /// </summary>
        private readonly IOrganizationServiceFactory _organizationServiceFactory;

        private IOrganizationService _defulatOrganizationService;
        private OrganizationServiceContext _defulatOrganizationServiceContext;
        #endregion
    }
}