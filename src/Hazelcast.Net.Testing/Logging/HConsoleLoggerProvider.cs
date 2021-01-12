// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Concurrent;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Testing.Logging
{
    public class HConsoleLoggerProvider : ITestingLoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();
        private readonly TestingLoggerOptions _options;

        private IExternalScopeProvider _scopeProvider;

        public HConsoleLoggerProvider(TestingLoggerOptions options = null)
        {
            _options = options ?? new TestingLoggerOptions();
        }

        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, name => new TestingLogger(this, name) { Options = _options });

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void WriteLog(LogMessageEntry entry)
        {
            HConsole.WriteLine(this, $"{entry.TimeStamp}{entry.LevelString}{entry.Message}");
        }

        public void Dispose()
        { }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        internal IExternalScopeProvider ScopeProvider
            => _scopeProvider ??= new LoggerExternalScopeProvider();
    }
}
