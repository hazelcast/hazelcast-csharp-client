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
    internal sealed class Logger
    {
        private static volatile ILoggerFactory loggerFactory;

        private static readonly object factoryLock = new object();

        public static ILogger GetLogger(Type clazz)
        {
            return GetLogger(clazz.FullName);
        }

        public static ILogger GetLogger(string name)
        {
            //noinspection DoubleCheckedLocking
            if (loggerFactory == null)
            {
                //noinspection SynchronizationOnStaticField
                lock (factoryLock)
                {
                    if (loggerFactory == null)
                    {
                        string loggerType = Environment.GetEnvironmentVariable("hazelcast.logging.type");
                        loggerFactory = NewLoggerFactory(loggerType);
                    }
                }
            }
            return loggerFactory.GetLogger(name);
        }

        public static ILoggerFactory NewLoggerFactory(string loggerType)
        {
            ILoggerFactory _loggerFactory = null;
            if ("console".Equals(loggerType))
            {
                _loggerFactory = new ConsoleLogFactory();
            }

            if (_loggerFactory == null)
            {
                _loggerFactory = (Debugger.IsAttached ? (ILoggerFactory)new TraceLogFactory() : new NoLogFactory());
                //_loggerFactory = new TraceLogFactory();
            }

            return _loggerFactory;
        }
    }
}