﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Testing.Logging
{
    public class TestingLogger : ILogger
    {
        private static readonly string _loglevelPadding = ": ";
        private static readonly string _messagePadding;
        private static readonly string _newLineWithMessagePadding;

        private readonly ITestingLoggerProvider _provider;
        private readonly string _name;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        static TestingLogger()
        {
            var logLevelString = GetLogLevelString(LogLevel.Information);
            _messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
            _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
        }

        public TestingLogger(ITestingLoggerProvider provider, string name)
        {
            _provider = provider;
            _name = name;
        }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        internal TestingLoggerOptions Options { get; set; }

        public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, _name, eventId.Id, message, exception);
            }
        }

        public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            var logBuilder = _logBuilder;
            _logBuilder = null;
            logBuilder ??= new StringBuilder();

            var entry = CreateDefaultLogMessage(logBuilder, logLevel, logName, eventId, message, exception);
            _provider.WriteLog(entry);

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024) logBuilder.Capacity = 1024;
            _logBuilder = logBuilder;
        }

        private LogMessageEntry CreateDefaultLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            // Example:
            // INFO: ConsoleApp.Program[10]
            //       Request received

            var logLevelString = GetLogLevelString(logLevel);
            // category and event id
            logBuilder.Append(_loglevelPadding);
            logBuilder.Append(logName);
            logBuilder.Append("[");
            logBuilder.Append(eventId);
            logBuilder.AppendLine("]");

            // scope information
            GetScopeInformation(logBuilder, multiLine: true);

            if (!string.IsNullOrEmpty(message))
            {
                // message
                logBuilder.Append(_messagePadding);

                var len = logBuilder.Length;
                logBuilder.AppendLine(message);
                logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
            }

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
            {
                // exception message
                logBuilder.AppendLine(exception.ToString());
            }

            //var timestampFormat = Options.TimestampFormat;

            return new LogMessageEntry
            {
                //TimeStamp = DateTime.Now.ToString(Options.TimestampFormat),
                Message = logBuilder.ToString(),
                LevelString = logLevelString
            };

            /*
            return new LogMessageEntry(
                message: logBuilder.ToString(),
                timeStamp: timestampFormat != null ? DateTime.Now.ToString(timestampFormat) : null,
                levelString: logLevelString,
                levelBackground: logLevelColors.Background,
                levelForeground: logLevelColors.Foreground,
                messageColor: DefaultConsoleColor,
                logAsError: logLevel >= Options.LogToStandardErrorThreshold
            );
            */
        }

        private void GetScopeInformation(StringBuilder stringBuilder, bool multiLine)
        {
            var scopeProvider = ScopeProvider;
            if (Options.IncludeScopes && scopeProvider != null)
            {
                var initialLength = stringBuilder.Length;

                scopeProvider.ForEachScope((scope, state) =>
                {
                    var (builder, paddAt) = state;
                    var padd = paddAt == builder.Length;
                    if (padd)
                    {
                        builder.Append(_messagePadding);
                        builder.Append("=> ");
                    }
                    else
                    {
                        builder.Append(" => ");
                    }
                    builder.Append(scope);
                }, (stringBuilder, multiLine ? initialLength : -1));

                if (stringBuilder.Length > initialLength && multiLine)
                {
                    stringBuilder.AppendLine();
                }
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
