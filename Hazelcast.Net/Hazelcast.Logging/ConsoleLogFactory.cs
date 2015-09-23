using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
    internal class ConsoleLogFactory : ILoggerFactory
    {

        public ConsoleLogFactory()
        {
        }

        public virtual ILogger GetLogger(string name)
        {
            return new ConsoleLogger(name);
        }

        internal class ConsoleLogger : AbstractLogger
        {
            private readonly string _name;

            internal ConsoleLogger(string name)
            {
                _name = name;
            }

            public override bool IsLoggable(LogLevel arg1)
            {
                return true;
            }

            public override void Log(LogLevel arg1, string message)
            {
                Console.WriteLine(GetDateFormat(arg1) + message);
            }

            public override void Log(LogLevel arg1, string message, Exception ex)
            {
                Console.WriteLine(GetDateFormat(arg1) + message + " ---- " + ex);
            }

            private string GetDateFormat(LogLevel logLevel)
            {
                return DateTime.Now.ToString("HH:mm:ss.fff") + " [" + logLevel.ToString().ToUpper() + "] - " + _name +
                       ": ";
            }

            public override void Log(TraceEventType logEvent)
            {
                
            }

            public override LogLevel GetLogLevel()
            {
                return LogLevel.Info;
            }
        }
    }
}