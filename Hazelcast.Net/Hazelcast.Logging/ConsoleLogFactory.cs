// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Config;

namespace Hazelcast.Logging
{
    internal class ConsoleLogFactory : ILoggerFactory
    {
        private readonly LogLevel _loggingLevel;

        public ConsoleLogFactory()
        {
            var logLevel = Environment.GetEnvironmentVariable("hazelcast.logging.level");
            if (logLevel == null)
            {
                _loggingLevel = LogLevel.All;
            }
            else
            {
                LogLevel level;
                if (Enum.TryParse(logLevel, true, out level))
                {
                    _loggingLevel = level;
                }
                else
                {
                    throw new ConfigurationException("Log level " + logLevel + " is invalid, the allowed values are " +
                                                     string.Join(", ", (LogLevel[]) Enum.GetValues(typeof (LogLevel))));
                }
            }
        }

        public virtual ILogger GetLogger(string name)
        {
            return new ConsoleLogger(name, _loggingLevel);
        }

        internal class ConsoleLogger : AbstractLogger
        {
            private readonly LogLevel _loggingLevel;
            private readonly string _name;

            internal ConsoleLogger(string name, LogLevel loggingLevel)
            {
                _name = name;
                _loggingLevel = loggingLevel;
            }

            public override LogLevel GetLevel()
            {
                return _loggingLevel;
            }

            public override bool IsLoggable(LogLevel arg1)
            {
                return _loggingLevel.IsGreaterThanOrEqualTo(arg1);
            }

            public override void Log(LogLevel arg1, string message)
            {
                if (!_loggingLevel.IsGreaterThanOrEqualTo(arg1)) return;
                Console.WriteLine(GetDateFormat(arg1) + message);
            }

            public override void Log(LogLevel arg1, string message, Exception ex)
            {
                if (!_loggingLevel.IsGreaterThanOrEqualTo(arg1)) return;
                Console.WriteLine(GetDateFormat(arg1) + message + " ---- " + ex);
            }

            private string GetDateFormat(LogLevel logLevel)
            {
                return DateTime.Now.ToString("HH:mm:ss.fff") + " [" + logLevel.ToString().ToUpper() + "] - [" +
                       Thread.CurrentThread.Name + ":" +
                       Thread.CurrentThread.ManagedThreadId + "] " + _name +
                       ": ";
            }
        }
    }
}