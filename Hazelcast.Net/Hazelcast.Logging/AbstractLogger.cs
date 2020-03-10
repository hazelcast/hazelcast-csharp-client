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

namespace Hazelcast.Logging
{
    /// <summary>
    ///     Abstract
    ///     <see cref="ILogger">ILogger</see>
    ///     implementation that provides implementations for the convenience methods like
    ///     finest,info,warning and severe.
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

        public virtual bool IsFinestEnabled => IsLoggable(LogLevel.Finest);

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

        public abstract bool IsLoggable(LogLevel arg1);

        public abstract void Log(LogLevel arg1, string arg2);

        public abstract void Log(LogLevel arg1, string arg2, Exception arg3);
        public abstract LogLevel LogLevel { get; }
    }
}