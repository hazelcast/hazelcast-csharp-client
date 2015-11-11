// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;

namespace Hazelcast.Logging
{
    internal class NoLogFactory : ILoggerFactory
    {
        private readonly ILogger _noLogger;

        public NoLogFactory()
        {
            _noLogger = new NoLogger();
        }

        public virtual ILogger GetLogger(string name)
        {
            return _noLogger;
        }

        internal class NoLogger : ILogger
        {
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

            public virtual LogLevel GetLevel()
            {
                return LogLevel.Off;
            }

            public bool IsLoggable(LogLevel level)
            {
                return false;
            }

            public virtual bool IsLoggable(TraceLevel level)
            {
                return false;
            }

            public virtual void Log(TraceEventType logEvent)
            {
            }

            public virtual void Log(TraceLevel level, string message)
            {
            }

            public virtual void Log(TraceLevel level, string message, Exception thrown)
            {
            }
        }
    }
}