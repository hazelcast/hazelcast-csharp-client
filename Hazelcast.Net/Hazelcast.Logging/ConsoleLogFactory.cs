using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
    internal class ConsoleLogFactory : ILoggerFactory
    {
        internal readonly ILogger logger;

        public ConsoleLogFactory()
        {
            logger = new ConsoleLogger(this);
        }

        public virtual ILogger GetLogger(string name)
        {
            return logger;
        }

        internal class ConsoleLogger : AbstractLogger
        {
            private readonly ConsoleLogFactory _enclosing;

            internal ConsoleLogger(ConsoleLogFactory _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public override bool IsLoggable(LogLevel arg1)
            {
                return true;
            }

            public override void Log(LogLevel arg1, string message)
            {
                Console.WriteLine(message);
            }

            public override void Log(LogLevel arg1, string message, Exception ex)
            {
                Console.WriteLine(message + "----"+ ex.StackTrace);
            }

            public override void Log(TraceEventType logEvent)
            {
                throw new NotImplementedException();
            }

            public override LogLevel GetLogLevel()
            {
                throw new NotImplementedException();
            }
        }
    }
}