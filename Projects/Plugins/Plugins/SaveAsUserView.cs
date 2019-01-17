namespace Crm.CommunitySupport.Plugins
{
    using Crm.CommunitySupport.Extensions;
    using System.Collections.Generic;

    /// <summary>
    /// A plugin that saves a copy of a system view as a user view.
    /// </summary>
    public class SaveAsUserView : Plugin
    {
        private const string ENTITY_USER_VIEW = "userquery";

        private static readonly Dictionary<string, string> AttributeMap = new Dictionary<string, string>
        {
            { "advancedgroupby", "advancedgroupby" },
            { "columnsetxml", "columnsetxml" },
            { "conditionalformatting", "conditionalformatting" },
            { "description", "description" },
            { "fetchxml", "fetchxml" },
            { "layoutjson", "layoutjson" },
            { "layoutxml", "layoutxml" },
            { "name", "name" },
            { "offlinesqlquery", "offlinesqlquery" },
            { "querytype", "querytype" },
            { "returnedtypecode", "returnedtypecode" },
        };

        public override void ExecutePlugin(PluginExecutionContext _)
        {
            // Input
            var systemView = _.Retrieve(_.GetTargetReference());
            var userView = systemView.MapAttributes(AttributeMap, ENTITY_USER_VIEW);
            userView.Id = _.Create(userView);

            // Output
            _.OutputParameters[ENTITY_USER_VIEW] = userView.ToEntityReference();
        }
    }
}
