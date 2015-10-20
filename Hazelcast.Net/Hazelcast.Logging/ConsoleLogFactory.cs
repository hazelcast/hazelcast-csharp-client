/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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