using System;
using System.Diagnostics;

namespace Hazelcast.Logging
{
    /// <summary>The Hazelcast logging interface.</summary>
    /// <remarks>
    ///     The Hazelcast logging interface. The reason if its existence is that Hazelcast doesn't want any dependencies
    ///     on concrete logging frameworks so it creates it own meta logging framework where existing logging frameworks can
    ///     be placed behind.
    /// </remarks>
    public interface ILogger
    {
        /// <summary>
        ///     Logs a message at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Info">Hazelcast.Net.Ext.Logging.Level.Info</see>
        ///     .
        /// </summary>
        /// <param name="message">the message to log.</param>
        void Info(string message);

        /// <summary>
        ///     Logs a message at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Finest">Hazelcast.Net.Ext.Logging.Level.Finest</see>
        ///     .
        /// </summary>
        /// <param name="message">the message to log.</param>
        void Finest(string message);

        /// <summary>
        ///     Logs a throwable at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Finest">Hazelcast.Net.Ext.Logging.Level.Finest</see>
        ///     .  The message of the Throwable will be the message.
        /// </summary>
        /// <param name="thrown">the Throwable to log.</param>
        void Finest(Exception thrown);

        /// <summary>
        ///     Logs message with associated throwable information at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Finest">Hazelcast.Net.Ext.Logging.Level.Finest</see>
        ///     .
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="thrown">the Throwable associated to the message.</param>
        void Finest(string message, Exception thrown);

        /// <summary>
        ///     Checks if the
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Finest">Hazelcast.Net.Ext.Logging.Level.Finest</see>
        ///     is enabled.
        /// </summary>
        /// <returns>true if enabled, false otherwise.</returns>
        bool IsFinestEnabled();

        /// <summary>
        ///     Logs a message at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Severe">Hazelcast.Net.Ext.Logging.Level.Severe</see>
        ///     .
        /// </summary>
        /// <param name="message">the message to log.</param>
        void Severe(string message);

        /// <summary>
        ///     Logs a throwable at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Severe">Hazelcast.Net.Ext.Logging.Level.Severe</see>
        ///     .  The message of the Throwable will be the message.
        /// </summary>
        /// <param name="thrown">the Throwable to log.</param>
        void Severe(Exception thrown);

        /// <summary>
        ///     Logs message with associated throwable information at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Severe">Hazelcast.Net.Ext.Logging.Level.Severe</see>
        ///     .
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="thrown">the Throwable associated to the message.</param>
        void Severe(string message, Exception thrown);

        /// <summary>
        ///     Logs a message at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Warning">Hazelcast.Net.Ext.Logging.Level.Warning</see>
        ///     .
        /// </summary>
        /// <param name="message">the message to log.</param>
        void Warning(string message);

        /// <summary>
        ///     Logs a throwable at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Warning">Hazelcast.Net.Ext.Logging.Level.Warning</see>
        ///     .  The message of the Throwable will be the message.
        /// </summary>
        /// <param name="thrown">the Throwable to log.</param>
        void Warning(Exception thrown);

        /// <summary>
        ///     Logs message with associated throwable information at
        ///     <see cref="Hazelcast.Net.Ext.Logging.Level.Warning">Hazelcast.Net.Ext.Logging.Level.Warning</see>
        ///     .
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="thrown">the Throwable associated to the message.</param>
        void Warning(string message, Exception thrown);

        /// <summary>Logs a message at the provided Level.</summary>
        /// <remarks>Logs a message at the provided Level.</remarks>
        /// <param name="level">the Level of logging.</param>
        /// <param name="message">the message to log.</param>
        void Log(LogLevel level, string message);

        /// <summary>Logs message with associated throwable information at the provided level.</summary>
        /// <remarks>Logs message with associated throwable information at the provided level.</remarks>
        /// <param name="message">the message to log</param>
        /// <param name="thrown">the Throwable associated to the message.</param>
        void Log(LogLevel level, string message, Exception thrown);

        /// <summary>Logs a LogEvent</summary>
        /// <param name="logEvent">the logEvent to log.</param>
        void Log(TraceEventType logEvent);

        /// <summary>Gets the logging Level.</summary>
        /// <remarks>Gets the logging Level.</remarks>
        /// <returns>the logging Level.</returns>
        LogLevel GetLevel();

        /// <summary>Checks if a message at the provided level is going to be logged by this logger.</summary>
        /// <remarks>Checks if a message at the provided level is going to be logged by this logger.</remarks>
        /// <param name="level">the log level.</param>
        /// <returns>true if this Logger will log messages for the provided level, false otherwise.</returns>
        bool IsLoggable(LogLevel level);
    }
}