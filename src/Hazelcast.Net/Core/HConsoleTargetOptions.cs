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

using System;
#if HZ_CONSOLE
using System.Globalization;
using System.Threading;
#else
#pragma warning disable CA1801 // Review unused parameters0
#pragma warning disable CA1822 // Mark members as static
#endif

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the options for a target.
    /// </summary>
#if HZ_CONSOLE_PUBLIC
    public
#else
    internal
#endif
    sealed class HConsoleTargetOptions
    {
#if HZ_CONSOLE
        private readonly HConsoleOptions _options;
        private bool _hasIndent;
        private bool _hasPrefix;
        private bool _hasLevel;
        private bool _hasTimeStamp;
#endif

#if HZ_CONSOLE
        /// <summary>
        /// Initializes a new instance of the <see cref="HConsoleTargetOptions"/> class.
        /// </summary>
        /// <param name="options"></param>
        public HConsoleTargetOptions(HConsoleOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Clones these options.
        /// </summary>
        /// <returns>A clone of these options.</returns>
        public HConsoleTargetOptions Clone()
        {
           var clone = new HConsoleTargetOptions(_options);
           if (_hasIndent) clone.SetIndent(Indent);
           if (_hasPrefix) clone.SetPrefix(Prefix);
           if (_hasLevel) clone.SetLevel(Level);
           if (_hasTimeStamp) clone.EnableTimeStamp(TimeStampEnabled, TimeStampOrigin);
           return clone;
        }

        /// <summary>
        /// Gets the default options.
        /// </summary>
        private static HConsoleTargetOptions Default { get; } = new HConsoleTargetOptions(null)
           .SetPrefix(default)
           .SetLevel(-1)
           .SetIndent(0)
           .EnableTimeStamp(false);
#endif

        #region Configure

        /// <summary>
        /// Configures default options.
        /// </summary>
        /// <returns>The default options to configure.</returns>
        public HConsoleTargetOptions Configure()
#if HZ_CONSOLE
            => Configure<object>();
#else
            => default;
#endif

        /// <summary>
        /// Configures options for a source type.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <returns>The options for the source type.</returns>
        public HConsoleTargetOptions Configure<TSource>()
#if HZ_CONSOLE
            => _options.Configure<TSource>();
#else
            => default;
#endif

        /// <summary>
        /// Configures options for a source type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <returns>The options for the source type.</returns>
        public HConsoleTargetOptions Configure(Type sourceType)
#if HZ_CONSOLE
            => _options.Configure(sourceType);
#else
            => default;
#endif

        /// <summary>
        /// Configures options for a source type.
        /// </summary>
        /// <param name="sourceTypeName">The name of the source type.</param>
        /// <returns>The options for the source type.</returns>
        public HConsoleTargetOptions Configure(string sourceTypeName)
#if HZ_CONSOLE
            => _options.Configure(sourceTypeName);
#else
            => default;
#endif

        /// <summary>
        /// Configures options for a source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The options for the source object.</returns>
        public HConsoleTargetOptions Configure(object source)
#if HZ_CONSOLE
            => _options.Configure(source);
#else
            => default;
#endif

        #endregion

        #region Options

        /// <summary>
        /// Sets the indentation level.
        /// </summary>
        /// <param name="indent">The indentation level.</param>
        /// <returns>This configuration object.</returns>

        public HConsoleTargetOptions SetIndent(int indent)
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
        public HConsoleTargetOptions ClearIndent()
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
        public HConsoleTargetOptions SetPrefix(string prefix)
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
        public HConsoleTargetOptions ClearPrefix()
        {
#if HZ_CONSOLE
            Prefix = default;
            _hasPrefix = false;
#endif
            return this;
        }

        /// <summary>
        /// Sets the log level.
        /// </summary>
        /// <param name="maxLevel">The log level.</param>
        /// <returns>This configuration object.</returns>
        public HConsoleTargetOptions SetLevel(int maxLevel)
        {
#if HZ_CONSOLE
            Level = maxLevel;
            _hasLevel = true;
#endif
            return this;
        }

        /// <summary>
        /// Clears the log level.
        /// </summary>
        /// <returns>This configuration object.</returns>
        public HConsoleTargetOptions ClearLevel()
        {
#if HZ_CONSOLE
            Level = default;
            _hasLevel = false;
#endif
            return this;
        }

        /// <summary>
        /// Enables or disables timestamp.
        /// </summary>
        /// <param name="enableTimeStamp">Whether to enable timestamps.</param>
        /// <param name="origin">An optional timestamp origin.</param>
        /// <returns>This configuration object.</returns>
        public HConsoleTargetOptions EnableTimeStamp(bool enableTimeStamp = true, DateTime origin = default)
        {
#if HZ_CONSOLE
            TimeStampEnabled = enableTimeStamp;
            TimeStampOrigin = origin;
            _hasTimeStamp = true;
#endif
            return this;
        }

        /// <summary>
        /// Clears timestamp.
        /// </summary>
        /// <returns>This configuration object.</returns>
        public HConsoleTargetOptions ClearTimeStamp()
        {
#if HZ_CONSOLE
            TimeStampEnabled = false;
            TimeStampOrigin = default;
            _hasTimeStamp = false;
#endif
            return this;
        }

        /// <summary>
        /// Sets the log level to -1 (never writes).
        /// </summary>
        /// <returns>This configuration object.</returns>
        public HConsoleTargetOptions SetMinLevel()
        {
#if HZ_CONSOLE
            Level = -1;
            _hasLevel = true;
#endif
            return this;
        }

        /// <summary>
        /// Sets the log level to int.MaxValue (always writes).
        /// </summary>
        /// <returns>This configuration object.</returns>
        public HConsoleTargetOptions SetMaxLevel()
        {
#if HZ_CONSOLE
            Level = int.MaxValue;
            _hasLevel = true;
#endif
            return this;
        }

#if HZ_CONSOLE
        /// <summary>
        /// Gets the indentation level.
        /// </summary>
        public int Indent { get; private set; }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Gets the log level.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Whether to enable timestamps.
        /// </summary>
        public bool TimeStampEnabled { get; private set; }

        /// <summary>
        /// Gets the timestamps origin.
        /// </summary>
        public DateTime TimeStampOrigin { get; private set; }
#endif

        /// <summary>
        /// Determines whether this configuration has an indentation level, a prefix and a maximal log level.
        /// </summary>
        public bool IsComplete
#if HZ_CONSOLE
            => _hasIndent && _hasPrefix && _hasLevel;
#else
            => true;
#endif

#if HZ_CONSOLE
        /// <summary>
        /// Merges another configuration into this configuration.
        /// </summary>
        /// <param name="options">The other configuration.</param>
        /// <returns>This configuration.</returns>
        public HConsoleTargetOptions Merge(HConsoleTargetOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!_hasIndent && options._hasIndent) SetIndent(options.Indent);
            if (!_hasPrefix && options._hasPrefix) SetPrefix(options.Prefix);
            if (!_hasLevel && options._hasLevel) SetLevel(options.Level);
            if (!_hasTimeStamp && options._hasTimeStamp) EnableTimeStamp(options.TimeStampEnabled, options.TimeStampOrigin);
            return this;
        }

        public HConsoleTargetOptions Complete()
            => Merge(Default);
#endif

        /// <summary>
        /// Gets the formatted prefix (with thread identifier etc).
        /// </summary>
        public string FormattedPrefix
        {
            get
            {
#if HZ_CONSOLE
                var timestamp = TimeStampEnabled ? (DateTime.Now - TimeStampOrigin).ToString("hhmmss\\.fff\\ ", CultureInfo.InvariantCulture) : "";
                return $"[{Environment.CurrentManagedThreadId:00}] {timestamp}{new string(' ', Indent)}{Prefix}: ";
#else
                return "";
#endif
            }
        }

        /// <summary>
        /// Determines whether a log level should be ignored.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>true if the level should be ignored; otherwise false.</returns>
        public bool Ignore(int level)
#if HZ_CONSOLE
            => level > Level;
#else
            => true;
#endif

        #endregion

        /// <inheritdoc />
        public override string ToString()
#if HZ_CONSOLE
            => $"indent = {(_hasIndent ? Indent.ToString(CultureInfo.InvariantCulture) : "?")}, prefix = {(_hasPrefix ? ("\"" + Prefix + "\"") : "?")}, level = {(_hasLevel ? Level.ToString(CultureInfo.InvariantCulture) : "?")}";
#else
            => "";
#endif
    }
}
