using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
    public class TraceLogFactory : ILoggerFactory
    {
        internal readonly ILogger logger;

        public TraceLogFactory()
        {
            logger = new TraceLogger(this);
        }

        public virtual ILogger GetLogger(string name)
        {
            return logger;
        }

        internal class TraceLogger : ILogger
        {
            private readonly TraceLogFactory _enclosing;

            internal TraceLogger(TraceLogFactory _enclosing)
            {
                this._enclosing = _enclosing;
            }

            public virtual void Finest(string message)
            {
                Trace.Write(message);
            }

            public virtual void Finest(string message, Exception thrown)
            {
                Trace.TraceError(thrown.Message);
            }

            public virtual void Finest(Exception thrown)
            {
                Trace.TraceError(thrown.Message);
            }

            public virtual bool IsFinestEnabled()
            {
                return true;
            }

            public virtual void Info(string message)
            {
                Trace.TraceInformation(message);
            }

            public virtual void Severe(string message)
            {
                Trace.TraceError(message);
            }

            public virtual void Severe(Exception thrown)
            {
                Trace.TraceError(thrown.Message);
            }

            public virtual void Severe(string message, Exception thrown)
            {
                Trace.TraceError(message);
            }

            public virtual void Warning(string message)
            {
                Trace.TraceWarning(message);
            }

            public virtual void Warning(Exception thrown)
            {
                Trace.TraceWarning(thrown.Message);
            }

            public virtual void Warning(string message, Exception thrown)
            {
                Trace.TraceWarning(message);
            }

            public void Log(LogLevel level, string message)
            {
                Trace.Write(message);
            }

            public void Log(LogLevel level, string message, Exception thrown)
            {
                Trace.Write(message);
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
                return true;
            }

            public virtual void Log(TraceLevel level, string message)
            {
                Trace.Write(message);
            }

            public virtual void Log(TraceLevel level, string message, Exception thrown)
            {
                Trace.Write(message);
            }

            public virtual bool IsLoggable(TraceLevel level)
            {
                return false;
            }
        }
    }
}