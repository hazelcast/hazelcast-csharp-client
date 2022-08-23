// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Testing.Logging
{
    /// <summary>
    /// Represents a logger provider that logs to a <see cref="StringBuilder"/>.
    /// </summary>
    public class StringBuilderLoggerProvider : ITestingLoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();
        private readonly object _textLock = new object();
        private readonly StringBuilder _text;
        private readonly TestingLoggerOptions _options;

        private IExternalScopeProvider _scopeProvider;

        public StringBuilderLoggerProvider(StringBuilder text, TestingLoggerOptions options = null)
        {
            _text = text;
            _options = options ?? new TestingLoggerOptions();
        }

        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, name => new TestingLogger(this, name) { Options = _options });

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void WriteLog(LogMessageEntry entry)
        {
            lock (_textLock)
            {
                _text.Append(entry.TimeStamp);
                _text.Append(entry.LevelString);
                _text.Append(entry.Message);
            }
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
