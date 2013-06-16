
using System.Windows;
using NLog;
using NLog.Config;
using NLog.Targets;
using UnderControl.Shared;

namespace UnderControl.Communication
{
    /// <summary>
    /// Configures logging and tracing using NLog's <c>LogManager</c>.
    /// </summary>
    internal static class LoggingConfigurator
    {
        /// <summary>
        /// Configures the logging and tracing using the given controller configuration.
        /// </summary>
        /// <param name="controllerConfiguration">The controller configuration.</param>
        public static void Configure(ControllerConfiguration controllerConfiguration)
        {
            // we better do this on the UI thread
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => Configure(controllerConfiguration));
                return;
            }

            // check if tracing should be enabled
            if (controllerConfiguration.EnableTracing && !string.IsNullOrEmpty(controllerConfiguration.TracingEndpointAddress))
            {
                // create the configuration
                var loggingConfig = new LoggingConfiguration();

                // create the target
                var serviceTarget = new LogReceiverWebServiceTarget();
                serviceTarget.ClientId = "WP7";
                serviceTarget.Name = "WebServiceTarget";
                serviceTarget.IncludeEventProperties = true;
                serviceTarget.UseBinaryEncoding = false;
                serviceTarget.EndpointAddress = controllerConfiguration.TracingEndpointAddress;

                // add parameters
                var parameter = new MethodCallParameter("time", "${time}");
                serviceTarget.Parameters.Add(parameter);
                parameter = new MethodCallParameter("threadid", "${threadid}");
                serviceTarget.Parameters.Add(parameter);

                // create the only rule
                var rule = new LoggingRule("*", LogLevel.Trace, serviceTarget);
                loggingConfig.LoggingRules.Add(rule);

                // set the config
                LogManager.Configuration = loggingConfig;
            }
            else
            {
                // turn off logging
                if (LogManager.Configuration != null && LogManager.Configuration.LoggingRules != null)
                {
                    LogManager.Configuration.LoggingRules.Clear();
                    LogManager.ReconfigExistingLoggers();
                }
            }
        }
    }
}
