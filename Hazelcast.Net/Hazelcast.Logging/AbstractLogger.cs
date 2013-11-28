using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
	/// <summary>
	/// Abstract
	/// <see cref="ILogger">ILogger</see>
	/// implementation that provides implementations for the convenience methods like
	/// finest,info,warning and severe.
	/// </summary>
	public abstract class AbstractLogger : ILogger
	{
		public virtual void Finest(string message)
		{
			Log(LogLevel.Finest, message);
		}

		public virtual void Finest(string message, Exception thrown)
		{
			Log(LogLevel.Finest, message, thrown);
		}

		public virtual void Finest(Exception thrown)
		{
			Log(LogLevel.Finest, thrown.Message, thrown);
		}

		public virtual bool IsFinestEnabled()
		{
			return IsLoggable(LogLevel.Finest);
		}

		public virtual void Info(string message)
		{
			Log(LogLevel.Info, message);
		}

		public virtual void Severe(string message)
		{
			Log(LogLevel.Severe, message);
		}

		public virtual void Severe(Exception thrown)
		{
			Log(LogLevel.Severe, thrown.Message, thrown);
		}

		public virtual void Severe(string message, Exception thrown)
		{
			Log(LogLevel.Severe, message, thrown);
		}

		public virtual void Warning(string message)
		{
			Log(LogLevel.Warning, message);
		}

		public virtual void Warning(Exception thrown)
		{
			Log(LogLevel.Warning, thrown.Message, thrown);
		}

		public virtual void Warning(string message, Exception thrown)
		{
			Log(LogLevel.Warning, message, thrown);
		}

		public abstract LogLevel GetLogLevel();

	    public LogLevel GetLevel()
	    {
	        throw new NotImplementedException();
	    }

	    public abstract bool IsLoggable(LogLevel arg1);

		public abstract void Log(LogLevel arg1, string arg2);

		public abstract void Log(LogLevel arg1, string arg2, Exception arg3);
	    public abstract void Log(TraceEventType logEvent);

	}
}
