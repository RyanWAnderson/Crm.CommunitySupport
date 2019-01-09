using Crm.CommunitySupport.Extensions;
using Microsoft.Xrm.Sdk;

namespace Crm.CommunitySupport.Plugins
{
    /// <summary>
    /// A plugin that does nothing in the plugin's body.
    /// </summary>
    public class TraceTarget : Plugin
    {
        public override void ExecutePlugin(PluginExecutionContext _)
        {
            var target = _.GetTarget<Entity>();

            _.Trace("Target: ", target?.ToTraceable() ?? "(null)");
        }
    }
}
