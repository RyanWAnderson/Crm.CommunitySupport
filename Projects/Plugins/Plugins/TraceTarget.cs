using Crm.CommunitySupport.Extensions;

namespace Crm.CommunitySupport.Plugins {
    /// <summary>
    /// A plugin that does nothing in the plugin's body.
    /// </summary>
    public class TraceTarget : BasePlugin {
        public override void ExecutePlugin(PluginContext _) {
            var target = _.Target;
            if (target == null) {
                _.Trace("Target: (null)");
                return;
            }

            _.Trace("Target: {0}", target.ToTraceable());
        }
    }
}
