using Crm.CommunitySupport.Extensions;
using Microsoft.Xrm.Sdk;
using System;
using System.Text;

namespace Crm.CommunitySupport.Plugins
{

    public abstract partial class Plugin : IPlugin
    {
        #region Constructor(s)
        public Plugin(string unsecure = "", string secure = "")
        {
            _configuration = new PluginConfiguration(unsecure, secure);
        }
        #endregion

        #region IPlugin support
        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            var _ = new PluginExecutionContext(serviceProvider);

            try
            {
                ExecutePluginWithTracesOnEntryAndExit(_);
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
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

        private void ExecutePluginWithTracesOnEntryAndExit(PluginExecutionContext _)
        {
            TraceEntryPoint(_, PluginTypeName);
            TraceTrigger(_, Configuration);

            var duration = Metrics.TimeAction(() =>
            {
                ExecutePlugin(_);
            });

            TraceExitPointWithDuration(_, PluginTypeName, duration);
        }

        /// <summary>
        /// Trace information about the SdkMessage that triggered the plugin
        /// </summary>
        /// <param name="_"></param>
        /// <param name="config"></param>
        private static void TraceTrigger(PluginExecutionContext _, PluginConfiguration config)
        {
            var blnTraceMessageStack = false;

            if (config.UnsecureDictionary.TryGetValue("TraceMessageStack", out var strTraceMessageStack))
            {
                bool.TryParse(config.UnsecureDictionary["TraceMessageStack"], out blnTraceMessageStack);
            }


            if (blnTraceMessageStack)
            {
                var messageStack = new StringBuilder();

                IPluginExecutionContext adam = _;
                do
                {
                    messageStack.AppendLine(adam.ToTraceableMessage());
                    adam = adam.ParentContext;
                } while (adam.ParentContext != null);
                adam = null;

                _.Trace(
                    "Plugin message stack: {0}{1}",
                    Environment.NewLine,
                    messageStack.ToString());

            }
            else
            {
                _.Trace(
                    "Plugin triggered by {0}",
                    _.ToTraceableMessage());
            }

        }

        private static void TraceEntryPoint(PluginExecutionContext _, string pluginTypeName)
        {
            _.Trace("Entering {0}.Execute(), Depth: {1}, Request Id: {2}, Correlation Id: {3}, Running as: {4}.",
                pluginTypeName,
                _.Depth.ToString(),
                _.RequestId.ToString(),
                _.CorrelationId.ToString(),
                _.InitiatingUserId.ToString());
        }

        private static void TraceExitPointWithDuration(PluginExecutionContext _, string pluginTypeName, TimeSpan duration)
        {
            _.Trace("Exiting {0}.Execute(), Correlation Id: {1}, Duration: {2:N2}ms.",
                pluginTypeName,
                _.CorrelationId.ToString(),
                duration.TotalMilliseconds);
        }
        #endregion

        public abstract void ExecutePlugin(PluginExecutionContext _);

        private readonly PluginConfiguration _configuration;
        public PluginConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    throw new InvalidPluginExecutionException(
                        "The plugin's Configuration was referenced, but the plugin did not call the base constructor in PluginBase." + Environment.NewLine +
                        "Example: public MyPlugin(string unsecure, string secure) : base(unsecure, secure) { }" + Environment.NewLine +
                        "Reference: https://msdn.microsoft.com/en-us/library/gg328263.aspx");
                }
                return _configuration;
            }
        }

        private string _pluginTypeName;

        private string PluginTypeName
        {
            // implemented as a lazy string instead of readonly string to eliminate the need for derived classes to call the base constructor
            get
            {
                if (_pluginTypeName == null)
                {
                    _pluginTypeName = GetType().FullName;
                }
                return _pluginTypeName;
            }
        }
    }
}
