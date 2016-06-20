using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

using Crm.CommunitySupport.Extensions;

namespace Crm.CommunitySupport.Plugins {
    /// <summary>
    /// A plugin that reduces the Target InputParameter to the delta, based on values present in the PreEntityImage
    /// Note: This can have unintended side effects if you are intentionally triggering plugins be setting a field to its current value and updating
    /// </summary>
    public class OptimizedUpdate : Plugin {
        #region Constructor(s)
        public OptimizedUpdate(string unsecure, string secure) : base(unsecure, secure) { }
        #endregion

        public override void ExecutePlugin(PluginExecutionContext _) {
            Entity target = null;
            Entity preImage = null;

            OptimizedUpdate.ValidatePluginRegistration(_);
            OptimizedUpdate.GetTarget(_, out target);
            OptimizedUpdate.GetPreImage(_, out preImage);
            OptimizedUpdate.ReduceTargetToDelta(_, target, preImage);
        }

        #region Stateless plugin logic
        private static void ValidatePluginRegistration(PluginExecutionContext _) {
            if (string.Compare(_.MessageName, "Update", ignoreCase: true) != 0 || !(_.Stage == PluginStage.PreValidation || _.Stage == PluginStage.PreOperation)) {
                _.Trace("This plugin only supports the Update message in Pre-Validation or Pre-Operation stages.");
                throw new InvalidPluginExecutionException("Invalid SdkMessageStep registration.");
            }
        }
        private static void GetTarget(PluginExecutionContext _, out Entity target) {
            _.Trace("Getting InputParameter['Target'].");

            target = _.GetTarget<Entity>();
            if (target == null) {
                throw new InvalidPluginExecutionException("Target is null.");
            }
        }
        private static void GetPreImage(PluginExecutionContext _, out Entity preImage) {
            _.Trace("Getting PreEntityImage.");

            preImage = _.GetPreImage<Entity>();
            if (preImage == null) {
                throw new InvalidPluginExecutionException("PreEntityImage is null. Register a single PreEntityImage on the message processing step, including all attributes that are to be compared.");
            }
        }
        private static void ReduceTargetToDelta(PluginExecutionContext _, Entity target, Entity preImage) {
            _.Trace("Reducing 'Target' Entity to delta, based on attribute values in the PreEntityImage.");
            IEnumerable<string> keysRemoved = target.ReduceToDelta(preImage);
            _.Trace("Removed attributes: {0}", string.Join(",", keysRemoved));
        }
        #endregion
    }

}
