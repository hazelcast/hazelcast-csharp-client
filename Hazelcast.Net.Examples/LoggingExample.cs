using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples
{
    public class LoggingExample : ExampleBase
    {
        public static void Run(params string[] args)
        {
            var options = BuildExampleOptions(args, keyValues: new Dictionary<string, string>
            {
                { "Logging:LogLevel:Hazelcast.Examples.LoggingExample.B", "Information" }
            });

            // FIXME: not  a singleton wtf? none of them are?!
            var loggerFactory = options.Logging.LoggerFactory.Create();

            var loggerA = loggerFactory.CreateLogger<A>();

            // default level is Debug - everything shows
            loggerA.LogDebug("debug.a");
            loggerA.LogInformation("info.a");
            loggerA.LogWarning("warning.a");

            var loggerB = loggerFactory.CreateLogger<B>();

            // level is info - first line is skipped
            loggerB.LogDebug("debug.b");
            loggerB.LogInformation("info.b");
            loggerB.LogWarning("warning.b");

            // flush logs!
            loggerFactory.Dispose();
        }

        public class A
        { }

        public class B
        { }
    }
}
