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
using System.Diagnostics;

namespace Hazelcast.Logging
{
    internal class TraceLogFactory : ILoggerFactory
    {
        internal readonly ILogger Logger;

        public TraceLogFactory()
        {
            Logger = new TraceLogger();
        }

        public virtual ILogger GetLogger(string name)
        {
            return Logger;
        }

        internal class TraceLogger : ILogger
        {
            public virtual void Finest(string message)
            {
                //TODO: Trace does not contain any native support for debug level
                Trace.TraceInformation(message);
            }

            public virtual void Finest(string message, Exception thrown)
            {
                Trace.TraceInformation(thrown.Message);
            }

            public virtual void Finest(Exception thrown)
            {
                Trace.TraceInformation(thrown.Message);
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
                Trace.TraceError(thrown.StackTrace);
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
                Trace.WriteLine(message);
            }

            public void Log(LogLevel level, string message, Exception thrown)
            {
                Trace.WriteLine(message);
            }

            public virtual LogLevel GetLevel()
            {
                return LogLevel.Off;
            }

            public bool IsLoggable(LogLevel level)
            {
                return true;
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
                Trace.WriteLine(message);
            }

            public virtual void Log(TraceLevel level, string message, Exception thrown)
            {
                Trace.WriteLine(message);
            }
        }
    }
}