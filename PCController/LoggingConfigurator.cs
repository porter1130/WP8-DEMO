using NLog;
using NLog.Config;
using NLog.Targets;

namespace PCController
{
    /// <summary>
    /// A helper class to configure logging and tracing using NLog's <c>LogManager</c>.
    /// </summary>
    public static class LoggingConfigurator
    {
        /// <summary>
        /// Configures NLog's logging with the given information.
        /// </summary>
        /// <param name="enableTracing">If set to <c>true</c> enables NLog's logging at "Trace" level. If set to <c>false</c> disables logging.</param>
        /// <param name="tracingEndpointAddress">The endpoint address of the WCF service that receives tracing messages.</param>
        public static void Configure(bool enableTracing, string tracingEndpointAddress)
        {
            // check if tracing should be enabled
            if (enableTracing && !string.IsNullOrEmpty(tracingEndpointAddress))
            {
                // create the configuration
                var loggingConfig = new LoggingConfiguration();

                // create the target
                var serviceTarget = new LogReceiverWebServiceTarget();
                serviceTarget.ClientId = "PC";
                serviceTarget.Name = "WebServiceTarget";
                serviceTarget.IncludeEventProperties = true;
                serviceTarget.UseBinaryEncoding = false;
                serviceTarget.EndpointAddress = tracingEndpointAddress;

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
                if (LogManager.Configuration != null && LogManager.Configuration.LoggingRules != null)
                {
                    LogManager.Configuration.LoggingRules.Clear();
                    LogManager.ReconfigExistingLoggers();
                }
            }
        }
    }
}
