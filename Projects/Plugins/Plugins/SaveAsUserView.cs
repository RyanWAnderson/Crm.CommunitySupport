using Crm.CommunitySupport.Extensions;
using System.Collections.Generic;

namespace Crm.CommunitySupport.Plugins
{
    /// <summary>
    /// A plugin that saves a copy of a system view as a user view.
    /// </summary>
    public class SaveAsUserView : Plugin
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
            { "savedqueryid", "userqueryid" },
        };

        public override void ExecutePlugin(PluginExecutionContext _)
        {
            // Input
            var systemView = _.Retrieve(_.PrimaryEntityName, _.PrimaryEntityId);
            var userView = systemView.MapAttributes(AttributeMap, "userquery");

            // Output
            _.OutputParameters["userqueryid"] = _.Create(userView);
        }
    }
}
