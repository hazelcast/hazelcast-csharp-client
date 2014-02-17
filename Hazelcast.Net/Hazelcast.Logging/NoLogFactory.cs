using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
    internal class NoLogFactory : ILoggerFactory
    {
        internal readonly ILogger noLogger;

        public NoLogFactory()
        {
            noLogger = new NoLogger(this);
        }

        public virtual ILogger GetLogger(string name)
        {
            return noLogger;
        }

        internal class NoLogger : ILogger
        {
            private readonly NoLogFactory _enclosing;

            internal NoLogger(NoLogFactory _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public virtual void Finest(string message)
            {
            }

            public virtual void Finest(string message, Exception thrown)
            {
            }

            public virtual void Finest(Exception thrown)
            {
            }

            public virtual bool IsFinestEnabled()
            {
                return false;
            }

            public virtual void Info(string message)
            {
            }

            public virtual void Severe(string message)
            {
            }

            public virtual void Severe(Exception thrown)
            {
            }

            public virtual void Severe(string message, Exception thrown)
            {
            }

            public virtual void Warning(string message)
            {
            }

            public virtual void Warning(Exception thrown)
            {
            }

            public virtual void Warning(string message, Exception thrown)
            {
            }

            public void Log(LogLevel level, string message)
            {
            }

            public void Log(LogLevel level, string message, Exception thrown)
            {
            }

            public virtual void Log(TraceEventType logEvent)
            {
            }

            public virtual LogLevel GetLevel()
            {
                return LogLevel.Off;
            }

            public bool IsLoggable(LogLevel level)
            {
                return false;
            }

            public virtual void Log(TraceLevel level, string message)
            {
            }

            public virtual void Log(TraceLevel level, string message, Exception thrown)
            {
            }

            public virtual bool IsLoggable(TraceLevel level)
            {
                return false;
            }
        }
    }
}