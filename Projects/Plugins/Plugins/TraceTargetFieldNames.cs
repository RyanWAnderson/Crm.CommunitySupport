using System.Linq;

using Microsoft.Xrm.Sdk;

using Crm.CommunitySupport.Extensions;

namespace Crm.CommunitySupport.Plugins {
    /// <summary>
    /// A plugin that does nothing in the plugin's body.
    /// </summary>
    public class TraceTargetFieldNames : BasePlugin {
        public override void ExecutePlugin(PluginContext _) {
            Entity target = _.Target;

            _.Trace("Updating {0}. Modified fields: {1}.",
                target.ToEntityReference().ToTraceable(),
                string.Join(",", target.Attributes.Keys.ToList()));
        }
    }
}
