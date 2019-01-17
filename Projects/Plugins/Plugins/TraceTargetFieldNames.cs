namespace Crm.CommunitySupport.Plugins
{
    using Crm.CommunitySupport.Extensions;
    using Microsoft.Xrm.Sdk;
    using System.Linq;

    /// <summary>
    /// A plugin that does nothing in the plugin's body.
    /// </summary>
    public class TraceTargetFieldNames : Plugin
    {
        public override void ExecutePlugin(PluginExecutionContext _)
        {
            var target = _.GetTarget<Entity>();

            _.Trace("Updating {0}. Modified fields: {1}.",
                target.ToEntityReference().ToTraceable(),
                string.Join(",", target.Attributes.Keys.ToList()));
        }
    }
}
