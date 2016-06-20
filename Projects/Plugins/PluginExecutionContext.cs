using System;
using System.Linq;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

using Crm.CommunitySupport.Extensions;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Crm.CommunitySupport.Plugins {
    public enum PluginStage {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40,
    }
    public enum ExecutionMode {
        Synchronous = 0,
        Sync = 0,
        Asynchronous = 1,
        Async = 1,
    }

    /// <summary>
    /// IPluginExecutionContext implementation with additional functionality for rapid development
    /// </summary>
    public partial class PluginExecutionContext : IPluginExecutionContext, ITracingService, IOrganizationServiceFactory, IOrganizationService {
        const string PARAM_TARGET = "Target";

        #region Constructor(s)
        public PluginExecutionContext(IServiceProvider serviceProvider) {
            _pluginExecutionContext = serviceProvider.GetService<IPluginExecutionContext>();
            _tracingService = new TimestampedTracingService(serviceProvider, OperationCreatedOn);
            _organizationServiceFactory = serviceProvider.GetService<IOrganizationServiceFactory>();
        }
        #endregion

        #region Interface support
        #region IPluginExecutionContext 
        private readonly IPluginExecutionContext _pluginExecutionContext;

        public PluginStage Stage {
            get {
                return (PluginStage)((IPluginExecutionContext)this).Stage;
            }
        }
        int IPluginExecutionContext.Stage {
            get {
                return this._pluginExecutionContext.Stage;
            }
        }

        public IPluginExecutionContext ParentContext {
            get {
                return this._pluginExecutionContext.ParentContext;
            }
        }

        public ExecutionMode Mode {
            get {
                return (ExecutionMode)((IPluginExecutionContext)this).Mode;
            }
        }
        int IExecutionContext.Mode {
            get {
                return this._pluginExecutionContext.Mode;
            }
        }

        public int IsolationMode {
            get {
                return this._pluginExecutionContext.IsolationMode;
            }
        }

        public int Depth {
            get {
                return this._pluginExecutionContext.Depth;
            }
        }

        public string MessageName {
            get {
                return this._pluginExecutionContext.MessageName;
            }
        }

        public string PrimaryEntityName {
            get {
                return this._pluginExecutionContext.PrimaryEntityName;
            }
        }

        public Guid? RequestId {
            get {
                return this._pluginExecutionContext.RequestId;
            }
        }

        public string SecondaryEntityName {
            get {
                return this._pluginExecutionContext.SecondaryEntityName;
            }
        }

        public ParameterCollection InputParameters {
            get {
                return this._pluginExecutionContext.InputParameters;
            }
        }

        public ParameterCollection OutputParameters {
            get {
                return this._pluginExecutionContext.OutputParameters;
            }
        }

        public ParameterCollection SharedVariables {
            get {
                return this._pluginExecutionContext.SharedVariables;
            }
        }

        public Guid UserId {
            get {
                return this._pluginExecutionContext.UserId;
            }
        }

        public Guid InitiatingUserId {
            get {
                return this._pluginExecutionContext.InitiatingUserId;
            }
        }

        public Guid BusinessUnitId {
            get {
                return this._pluginExecutionContext.BusinessUnitId;
            }
        }

        public Guid OrganizationId {
            get {
                return this._pluginExecutionContext.OrganizationId;
            }
        }

        public string OrganizationName {
            get {
                return this._pluginExecutionContext.OrganizationName;
            }
        }

        public Guid PrimaryEntityId {
            get {
                return this._pluginExecutionContext.PrimaryEntityId;
            }
        }

        public EntityImageCollection PreEntityImages {
            get {
                return this._pluginExecutionContext.PreEntityImages;
            }
        }

        public EntityImageCollection PostEntityImages {
            get {
                return this._pluginExecutionContext.PostEntityImages;
            }
        }

        public EntityReference OwningExtension {
            get {
                return this._pluginExecutionContext.OwningExtension;
            }
        }

        public Guid CorrelationId {
            get {
                return this._pluginExecutionContext.CorrelationId;
            }
        }

        public bool IsExecutingOffline {
            get {
                return this._pluginExecutionContext.IsExecutingOffline;
            }
        }

        public bool IsOfflinePlayback {
            get {
                return this._pluginExecutionContext.IsOfflinePlayback;
            }
        }

        public bool IsInTransaction {
            get {
                return this._pluginExecutionContext.IsInTransaction;
            }
        }

        public Guid OperationId {
            get {
                return this._pluginExecutionContext.OperationId;
            }
        }

        public DateTime OperationCreatedOn {
            get {
                return this._pluginExecutionContext.OperationCreatedOn;
            }
        }
        #endregion
        #region ITracingService 
        private readonly ITracingService _tracingService;
        /// <summary>
        /// Trace a formatted message, per the implemented ITracingService
        /// </summary>
        public void Trace(string format, params object[] args) {
            try {
                _tracingService.Trace(format, args);
            }
            catch { }
        }
        #endregion
        #region IOrganizationServiceFactory 
        private readonly IOrganizationServiceFactory _organizationServiceFactory;
        /// <summary>
        /// Create a new OrganizationService under the given user context.
        /// </summary>
        /// <param name="userId">A null value indicates the SYSTEM user, Guid.Empty indicates the user context of the given pipeline. Any other value indicates a specific user id.</param>
        /// <returns></returns>
        public IOrganizationService CreateOrganizationService(Guid? userId = null) {
            return _organizationServiceFactory.CreateOrganizationService(userId);
        }
        #endregion
        #region IOrganizationService
        private IOrganizationService _defaultOrganizationService;
        /// <summary>
        /// Lazyily-initialized IOrganizationService for the User in the current security context
        /// </summary>
        IOrganizationService DefaultOrganizationService {
            get {
                if (_defaultOrganizationService == null) {
                    DefaultOrganizationService = CreateOrganizationService(InitiatingUserId);
                }
                return _defaultOrganizationService;
            }
            set {
                _defaultOrganizationService = value;
            }
        }
        public EntityCollection RetrieveMultiple(QueryBase query) {
            return Execute<RetrieveMultipleResponse>(
                new RetrieveMultipleRequest() {
                    Query = query
                }).EntityCollection;
        }

        public Guid Create(Entity entity) {
            return Execute<CreateResponse>(
                new CreateRequest() {
                    Target = entity
                }).id;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) {
            return Execute<RetrieveResponse>(
                new RetrieveRequest() {
                    Target = new EntityReference(entityName, id),
                    ColumnSet = columnSet
                }).Entity;
        }

        public void Update(Entity entity) {
            Execute<UpdateResponse>(
                new UpdateRequest() {
                    Target = entity
                });
        }

        public void Delete(string entityName, Guid id) {
            Execute<DeleteResponse>(
                new DeleteRequest() {
                    Target = new EntityReference(entityName, id)
                });
        }

        public OrganizationResponse Execute(OrganizationRequest request) {
            Trace("Issuing {0} request.", request.RequestName);
            var response = this.DefaultOrganizationService.Execute(request);
            Trace("{0} response received.", request.RequestName);
            return response;
        }
        public TOrganizationResponse Execute<TOrganizationResponse>(OrganizationRequest request) where TOrganizationResponse : OrganizationResponse {
            return (TOrganizationResponse)Execute(request);
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) {
            Execute<AssociateResponse>(
                new AssociateRequest() {
                    Target = new EntityReference(entityName, entityId),
                    Relationship = relationship,
                    RelatedEntities = relatedEntities
                });
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) {
            Execute<DisassociateResponse>(
                new DisassociateRequest() {
                    Target = new EntityReference(entityName, entityId),
                    Relationship = relationship,
                    RelatedEntities = relatedEntities
                });
        }
        #endregion
        #endregion

        #region Additional Crm.Messages support
        public UpsertResponse Upsert(Entity entity) {
            return Execute<UpsertResponse>(
                new UpsertRequest() {
                    Target = entity
                });
        }
        public OrganizationResponseCollection ExecuteTransaction(OrganizationRequestCollection requests, bool? returnResponses) {
            return Execute<ExecuteTransactionResponse>(
                new ExecuteTransactionRequest() {
                    Requests = requests,
                    ReturnResponses = returnResponses
                }).Responses;
        }
        public Guid ExecuteWorkflow(Guid workflowId, Guid entityId = default(Guid), ParameterCollection parameters = default(ParameterCollection)) {
            return Execute<ExecuteWorkflowResponse>(
                new ExecuteWorkflowRequest() {
                    WorkflowId = workflowId,
                    EntityId = entityId,
                    Parameters = parameters
                }).Id;
        }
        public QueryExpression FetchXmlToQuery(FetchExpression fetchExpression) {
            return Execute<FetchXmlToQueryExpressionResponse>(
                new FetchXmlToQueryExpressionRequest() {
                    FetchXml = fetchExpression.Query
                }).Query;
        }
        #endregion

        /// <summary>
        /// Get a PreEntityImage based on imageName. If no imageName is provided, assume that there is exactly 1 image registered.
        /// </summary>
        public TEntity GetPreImage<TEntity>(string imageName = "") where TEntity : Entity {
            return PreEntityImages.GetImage<TEntity>(imageName);
        }

        /// <summary>
        /// Get a PostEntityImage based on imageName. If no imageName is provided, assume that there is exactly 1 image registered.
        /// </summary>
        public TEntity GetPostImage<TEntity>(string imageName = "") where TEntity : Entity {
            return PostEntityImages.GetImage<TEntity>(imageName);
        }

        /// <summary>
        /// Lazyily-initialized OrganizationServiceContext based off DefaultOrganizationSerivce
        /// </summary>
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

        /// <summary>
        /// Get the strongly-typed Target of the execution context
        /// </summary>
        public TEntity GetTarget<TEntity>() where TEntity : Entity {
            object target = null;
            InputParameters.TryGetValue(PARAM_TARGET, out target);
            return (target as Entity)?.ToEntity<TEntity>();
        }

        /// <summary>
        /// Get the Target Entity/EntityReference of the context as an EntityReference.
        /// </summary>
        public EntityReference GetTargetReference() {
            object target = null;
            InputParameters.TryGetValue(PARAM_TARGET, out target);

            if (target is Entity) {
                target = ((Entity)target).ToEntityReference();
            }

            return target as EntityReference;
        }

        private OrganizationServiceContext _defulatOrganizationServiceContext;
    }
}