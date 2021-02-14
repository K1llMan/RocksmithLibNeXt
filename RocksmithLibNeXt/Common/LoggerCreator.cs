using System;

using Microsoft.Extensions.Logging;

namespace RocksmithLibNeXt.Common
{
    /// <summary>
    /// Static class for creating logger
    /// </summary>
    public class LoggerCreator
    {
        public static ILogger Create(Type t)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddSimpleConsole(o => {
                        o.TimestampFormat = "HH:mm:ss ";
                        o.IncludeScopes = true;
                        o.SingleLine = false;
                    })
                    //.AddFilter("Microsoft", LogLevel.Trace)
                    //.AddFilter("System", LogLevel.Trace)
                    //.AddFilter("LoggingConsoleApp.Program", LogLevel.Trace)
                    .AddConsole();
            });

            return loggerFactory.CreateLogger(t);
        }
    }
}
