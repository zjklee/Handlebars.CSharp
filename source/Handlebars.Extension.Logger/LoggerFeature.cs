using System;
using HandlebarsDotNet.Adapters;
using HandlebarsDotNet.Compiler.Structure.Path;
using HandlebarsDotNet.Extension.Logging;
using HandlebarsDotNet.Features;
using HandlebarsDotNet.Helpers;

namespace HandlebarsDotNet.Extension.Logger
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="level"></param>
    /// <param name="format"></param>
    public delegate void Log(object[] arguments, LoggingLevel level, Func<object[], string> format);

    internal class LoggerFeature : IFeature
    {
        private readonly Log _logger;

        public LoggerFeature(Log logger)
        {
            _logger = logger;
        }
        
        public void OnCompiling(ICompiledHandlebarsConfiguration configuration)
        {
            var logPathInfo = configuration.PathInfoStore.GetOrAdd("log");
            if (configuration.Helpers.TryGetValue(logPathInfo, out var logger))
            {
                var originalLogger = logger.Value as LoggerHelperDescriptor;
                configuration.Helpers[logPathInfo].Value = new LoggerHelperDescriptor(_logger, originalLogger);
            }
            else
            {
                configuration.Helpers[logPathInfo] = new Ref<HelperDescriptorBase>(new LoggerHelperDescriptor(_logger));   
            }
        }

        public void CompilationCompleted()
        {
        }
    }
}