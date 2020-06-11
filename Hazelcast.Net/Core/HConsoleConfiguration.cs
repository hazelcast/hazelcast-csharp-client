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
using System.Globalization;
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a logging configuration.
    /// </summary>
    public sealed class HConsoleConfiguration
    {
#if HZ_CONSOLE
        private bool _hasIndent;
        private bool _hasPrefix;
        private bool _hasMaxLevel;
#endif

        /// <summary>
        /// Sets the indentation level.
        /// </summary>
        /// <param name="indent">The indentation level.</param>
        /// <returns>This configuration object.</returns>
        public HConsoleConfiguration SetIndent(int indent)
        {
#if HZ_CONSOLE
            Indent = indent;
            _hasIndent = true;
#endif
            return this;
        }

        /// <summary>
        /// Clears the indentation level.
        /// </summary>
        /// <returns>This configuration object.</returns>
        public HConsoleConfiguration ClearIndent()
        {
#if HZ_CONSOLE
            Indent = default;
            _hasIndent = false;
#endif
            return this;
        }

        /// <summary>
        /// Sets the prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>This configuration object.</returns>
        public HConsoleConfiguration SetPrefix(string prefix)
        {
#if HZ_CONSOLE
            Prefix = prefix;
            _hasPrefix = true;
#endif
            return this;
        }

        /// <summary>
        /// Clears the prefix.
        /// </summary>
        /// <returns>This configuration object.</returns>
        public HConsoleConfiguration ClearPrefix()
        {
#if HZ_CONSOLE
            Prefix = default;
            _hasPrefix = false;
#endif
            return this;
        }

        /// <summary>
        /// Sets the maximal log level.
        /// </summary>
        /// <param name="maxLevel">The maximal log level.</param>
        /// <returns>This configuration object.</returns>
        public HConsoleConfiguration SetMaxLevel(int maxLevel)
        {
#if HZ_CONSOLE
            MaxLevel = maxLevel;
            _hasMaxLevel = true;
#endif
            return this;
        }

        /// <summary>
        /// Clears the maximal log level.
        /// </summary>
        /// <returns>This configuration object.</returns>
        public HConsoleConfiguration ClearMaxLevel()
        {
#if HZ_CONSOLE
            MaxLevel = default;
            _hasMaxLevel = false;
#endif
            return this;
        }

#if HZ_CONSOLE
        /// <summary>
        /// Gets the indentation level.
        /// </summary>
        internal int Indent { get; private set; }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        internal string Prefix { get; private set; }

        /// <summary>
        /// Gets the maximal log level.
        /// </summary>
        internal int MaxLevel { get; private set; }
#endif

        /// <summary>
        /// Determines whether this configuration has an indentation level, a prefix and a maximal log level.
        /// </summary>
        internal bool IsComplete
#if HZ_CONSOLE
            => _hasIndent && _hasPrefix && _hasMaxLevel;
#else
                => true;
#endif

#if HZ_CONSOLE
        /// <summary>
        /// Merges another configuration into this configuration.
        /// </summary>
        /// <param name="config">The other configuration.</param>
        /// <returns>This configuration.</returns>
        public HConsoleConfiguration Merge(HConsoleConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (!_hasIndent && config._hasIndent) SetIndent(config.Indent);
            if (!_hasPrefix && config._hasPrefix) SetPrefix(config.Prefix);
            if (!_hasMaxLevel && config._hasMaxLevel) SetMaxLevel(config.MaxLevel);
            return this;
        }
#endif

        /// <summary>
        /// Gets the formatted prefix (with thread identifier etc).
        /// </summary>
        public string FormattedPrefix
#if HZ_CONSOLE
            => $"{new string(' ', Indent)}[{Thread.CurrentThread.ManagedThreadId:00}] {Prefix}: ";
#else
                => "";
#endif

        /// <summary>
        /// Determines whether a log level should be ignored.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>true if the level should be ignored; otherwise false.</returns>
        public bool Ignore(int level)
#if HZ_CONSOLE
            => level > MaxLevel;
#else
                => true;
#endif

        /// <inheritdoc />
        public override string ToString()
#if HZ_CONSOLE
            => $"{{Config: {(_hasIndent ? Indent.ToString(CultureInfo.InvariantCulture) : "?")}, {(_hasPrefix ? ("\"" + Prefix + "\"") : "?")}, {(_hasMaxLevel ? MaxLevel.ToString(CultureInfo.InvariantCulture) : "?")}}}";
#else
                => "";
#endif
    }
}