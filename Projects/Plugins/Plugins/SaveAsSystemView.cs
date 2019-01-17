namespace Crm.CommunitySupport.Plugins
{
    using Crm.CommunitySupport.Extensions;
    using Microsoft.Xrm.Sdk;
    using System.Collections.Generic;

    /// <summary>
    /// A plugin that saves a copy of user view as a system view.
    /// </summary>
    public class SaveAsSystemView : Plugin
    {
        private const string ENTITY_SYSTEM_VIEW = "savedquery";

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
            var userView = _.Retrieve(_.GetTargetReference());
            var systemView = ConvertToSystemView(userView);
            systemView.Id = _.Create(systemView);

            // Output
            _.OutputParameters[ENTITY_SYSTEM_VIEW] = systemView.ToEntityReference();
        }

        private static Entity ConvertToSystemView(Entity userView)
        {
            var systemView = userView.MapAttributes(AttributeMap, ENTITY_SYSTEM_VIEW);

            systemView["canbedeleted"] = true;
            systemView["iscustomizable"] = true;
            systemView["isdefault"] = false;
            systemView["isquickfindquery"] = false;

            return systemView;
        }
    }
}
