using System;
using Microsoft.Xrm.Sdk;
using System.Text;

using Crm.CommunitySupport.Extensions;

namespace Crm.CommunitySupport.Plugins {

    public abstract partial class BasePlugin : IPlugin {
        #region Constructor(s)
        public BasePlugin(string unsecure = "", string secure = "") {
            _configuration = new PluginConfiguration(unsecure, secure);
        }
        #endregion

        #region IPlugin Implementation
        void IPlugin.Execute(IServiceProvider serviceProvider) {
            PluginContext _ = new PluginContext(serviceProvider);

            try {
                this.ExecutePluginWithTracesOnEntryAndExit(_);
            }
            catch (Exception ex) {
                _.Trace("!! Exception caught, plugin aborting.");

                throw new InvalidPluginExecutionException(
                    string.Format(
                        "Plugin '{0}' failed to execute, returning the error: {1}",
                        PluginTypeName,
                        ex.Message
                    ),
                    ex);
            }
        }

        private void ExecutePluginWithTracesOnEntryAndExit(PluginContext _) {
            TraceEntryPoint(_, this.PluginTypeName);
            TraceTrigger(_, this.Configuration);

            TimeSpan duration = Metrics.TimeAction(() => {
                ExecutePlugin(_);
            });

            TraceExitPointWithDuration(_, this.PluginTypeName, duration);
        }
        
        /// <summary>
        /// Trace information about the SdkMessage that triggered the plugin
        /// </summary>
        /// <param name="_"></param>
        /// <param name="config"></param>
        private static void TraceTrigger(PluginContext _, PluginConfiguration config) {
            bool blnTraceMessageStack;
            bool.TryParse(config.UnsecureDictionary["TraceMessageStack"], out blnTraceMessageStack);

            if (blnTraceMessageStack) {
                StringBuilder messageStack = new StringBuilder();

                IPluginExecutionContext adam = _.XrmContext;
                do {
                    messageStack.AppendLine(adam.ToTraceableMessage());
                    adam = adam.ParentContext;
                } while (adam.ParentContext != null);
                adam = null;

                _.Trace(
                    "Plugin message stack: {0}{1}",
                    Environment.NewLine,
                    messageStack.ToString());

            } else {
                _.Trace(
                    "Plugin triggered by {0}",
                    _.XrmContext.ToTraceableMessage());
            }

        }

        private static void TraceEntryPoint(PluginContext _, string pluginTypeName) {
            _.Trace("Entering {0}.Execute(), Depth: {1}, Request Id: {2}, Correlation Id: {3}, Running as: {4}.",
                pluginTypeName,
                _.XrmContext.Depth.ToString(),
                _.XrmContext.RequestId.ToString(),
                _.XrmContext.CorrelationId.ToString(),
                _.XrmContext.InitiatingUserId.ToString());
        }

        private static void TraceExitPointWithDuration(PluginContext _, string pluginTypeName, TimeSpan duration) {
            _.Trace("Exiting {0}.Execute(), Correlation Id: {1}, Duration: {2:N2}ms.",
                pluginTypeName,
                _.XrmContext.CorrelationId.ToString(),
                duration.TotalMilliseconds);
        }
        #endregion

        public abstract void ExecutePlugin(PluginContext _);

        public PluginConfiguration Configuration {
            get {
                if (_configuration == null) {
                    throw new InvalidPluginExecutionException(
                        "The plugin's Configuration was referenced, but the plugin did not call the base constructor in PluginBase." + Environment.NewLine +
                        "Example: public MyPlugin(string unsecure, string secure) : base(unsecure, secure) { }" + Environment.NewLine +
                        "Reference: https://msdn.microsoft.com/en-us/library/gg328263.aspx");
                }
                return _configuration;
            }
        }
        string PluginTypeName {
            // implemented as a lazy string instead of readonly string to eliminate the need for derived classes to call the base constructor
            get {
                if (_pluginTypeName == null) {
                    _pluginTypeName = this.GetType().FullName;
                }
                return _pluginTypeName;
            }
        }
        readonly PluginConfiguration _configuration;
        string _pluginTypeName;
    }
}
