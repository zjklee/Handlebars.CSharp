using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandlebarsDotNet.Extension.Logger;
using HandlebarsDotNet.Helpers;

namespace HandlebarsDotNet.Extension.Logging
{
    internal sealed class LoggerHelperDescriptor : HelperDescriptor
    {
        private readonly Log _logger;
        private readonly LoggerHelperDescriptor? _baseLogger;
        private readonly Func<object[], string> _defaultFormatter = objects => string.Join("; ", objects);

        public LoggerHelperDescriptor(Log logger, LoggerHelperDescriptor? baseLogger = null) : base("log")
        {
            _logger = logger;
            _baseLogger = baseLogger;
        }

        public override void Invoke(TextWriter output, object context, params object[] arguments)
        {
            _baseLogger?.Invoke(output, context, arguments);
            
            var logLevel = LoggingLevel.Info;
            var formatter = _defaultFormatter;

            var logArguments = arguments;
            if (arguments.Last() is IDictionary<string, object> hash)
            {
                logArguments = arguments.Take(arguments.Length - 1).ToArray();
                if(hash.TryGetValue("level", out var level))
                {
                    if(Enum.TryParse<LoggingLevel>(level.ToString(), true, out var hbLevel))
                    {
                        logLevel = hbLevel;
                    }
                }

                if (hash.TryGetValue("format", out var format))
                {
                    var formatString = format.ToString()
                        .Replace("[", "{")
                        .Replace("]", "}");
                    
                    formatter = objects => string.Format(formatString, objects);
                }
            }

            _logger(logArguments, logLevel, formatter);
        }
    }
}