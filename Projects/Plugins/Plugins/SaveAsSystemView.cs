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
            { "userqueryid", "savedqueryid" },
        };

        public override void ExecutePlugin(PluginExecutionContext _)
        {
            // Input
            var userView = _.Retrieve(_.PrimaryEntityName, _.PrimaryEntityId,);
            var systemView = ConvertToSystemView(userView);

            // Output
            _.OutputParameters["savedqueryid"] = _.Create(systemView);
        }

        private static Entity ConvertToSystemView(Entity userView)
        {
            var systemView = userView.MapAttributes(AttributeMap, "savedquery");

            systemView["canbedeleted"] = true;
            systemView["iscustomizable"] = true;
            systemView["isdefault"] = false;
            systemView["isquickfindquery"] = false;

            return systemView;
        }
    }
}
