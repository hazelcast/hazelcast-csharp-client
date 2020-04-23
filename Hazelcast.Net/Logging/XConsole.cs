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
using System.Threading;

#if XCONSOLE
using System.Collections.Generic;
#endif

namespace Hazelcast.Logging
{
    /// <summary>
    /// Provides a console for troubleshooting.
    /// </summary>
    /// <remarks>
    /// <para>To enable the console, define XCONSOLE. Otherwise, none of the console
    /// method invocations will be compiled (thanks to <see cref="ConditionalAttribute"/>)
    /// and therefore the impact on actual (production) code will be null.</para>
    /// </remarks>
    internal static class XConsole
    {
#if XCONSOLE
        private static readonly Dictionary<object, Config> SourceConfigs = new Dictionary<object, Config>();
        private static readonly Dictionary<Type, Config> TypeConfigs = new Dictionary<Type, Config>();

        private static Config GetConfig(object source)
        {
            if (!SourceConfigs.TryGetValue(source, out var config)) config = new Config();

            var type = source.GetType();
            while (type != null && !config.IsComplete)
            {
                if (TypeConfigs.TryGetValue(type, out var c)) config = config.Merge(c);
                type = type.BaseType;
            }

            return config;
        }
#endif

        /// <summary>
        /// Represents a logging configuration.
        /// </summary>
        public sealed class Config
        {
#if XCONSOLE
            private bool _hasIndent;
            private bool _hasPrefix;
            private bool _hasMaxLevel;
#endif

            /// <summary>
            /// Sets the indentation level.
            /// </summary>
            /// <param name="indent">The indentation level.</param>
            /// <returns>This configuration object.</returns>
            public Config SetIndent(int indent)
            {
#if XCONSOLE
                Indent = indent;
                _hasIndent = true;
#endif
                return this;
            }

            /// <summary>
            /// Clears the indentation level.
            /// </summary>
            /// <returns>This configuration object.</returns>
            public Config ClearIndent()
            {
#if XCONSOLE
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
            public Config SetPrefix(string prefix)
            {
#if XCONSOLE
                Prefix = prefix;
                _hasPrefix = true;
#endif
                return this;
            }

            /// <summary>
            /// Clears the prefix.
            /// </summary>
            /// <returns>This configuration object.</returns>
            public Config ClearPrefix()
            {
#if XCONSOLE
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
            public Config SetMaxLevel(int maxLevel)
            {
#if XCONSOLE
                MaxLevel = maxLevel;
                _hasMaxLevel = true;
#endif
                return this;
            }

            /// <summary>
            /// Clears the maximal log level.
            /// </summary>
            /// <returns>This configuration object.</returns>
            public Config ClearMaxLevel()
            {
#if XCONSOLE
                MaxLevel = default;
                _hasMaxLevel = false;
#endif
                return this;
            }

#if XCONSOLE
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
#if XCONSOLE
                => _hasIndent && _hasPrefix && _hasMaxLevel;
#else
                => true;
#endif

#if XCONSOLE
            /// <summary>
            /// Merges another configuration into this configuration.
            /// </summary>
            /// <param name="config">The other configuration.</param>
            /// <returns>This configuration.</returns>
            public Config Merge(Config config)
            {
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
#if XCONSOLE
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
#if XCONSOLE
                => level > MaxLevel;
#else
                => true;
#endif

            /// <inheritdoc />
            public override string ToString()
#if XCONSOLE
                => $"{{Config: {(_hasIndent?Indent.ToString():"?")}, {(_hasPrefix?("\""+Prefix+"\""):"?")}, {(_hasMaxLevel?MaxLevel.ToString():"?")}}}";
#else
                => "";
#endif
        }

        // NOTES
        //
        // The class above *must* exist regardless of the XCONSOLE define, as
        // it is exposed by the methods below, so that the code can compile,
        // and even though these methods may not be compiled (see below).
        // However, to reduce its weight, we can simplify the class body when
        // XCONSOLE is not defined.
        //
        // Methods below *must* exist regardless of the XCONSOLE define, so that
        // the code can compile, but due to the [Conditional] attributes, their
        // invocations will *not* be compiled = zero cost.
        // Nevertheless, to reduce their weight, we can simplify their bodies
        // when XCONSOLE is not defined.
        // The 'Lines' method cannot be marked [Conditional] because it returns
        // a non-void value. However, it should always be invoked from within
        // a call to WriteLine, which will *not* be compiled anyways.

        /// <summary>
        /// Configure the console for a source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="configure">A action to configure the console.</param>
        [Conditional("XCONSOLE")]
        public static void Configure(object source, Action<Config> configure)
        {
#if XCONSOLE
            if (!SourceConfigs.TryGetValue(source, out var info))
                info = SourceConfigs[source] = new Config();
            configure(info);
#endif
        }

        /// <summary>
        /// Configure the console for a type of objects.
        /// </summary>
        /// <typeparam name="TObject">The type of objects.</typeparam>
        /// <param name="configure">A action to configure the console.</param>
        [Conditional("XCONSOLE")]
        public static void Configure<TObject>(Action<Config> configure)
        {
#if XCONSOLE
            var type = typeof(TObject);
            if (!TypeConfigs.TryGetValue(type, out var info))
                info = TypeConfigs[type] = new Config();
            configure(info);
#endif
        }

        /// <summary>
        /// Writes a line.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="text">The text to write.</param>
        [Conditional("XCONSOLE")]
        public static void WriteLine(object source, string text)
        {
#if XCONSOLE
            var config = GetConfig(source);
            Console.WriteLine(config.FormattedPrefix + text);
#endif
        }

        /// <summary>
        /// Writes a line at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="text">The text to write.</param>
        [Conditional("XCONSOLE")]
        public static void WriteLine(object source, int level, string text)
        {
#if XCONSOLE
            var config = GetConfig(source);
            if (config.Ignore(level)) return;
            Console.WriteLine(config.FormattedPrefix + text);
#endif
        }

        /// <summary>
        /// Writes a line.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="format">The line format.</param>
        /// <param name="args">The line arguments.</param>
        [Conditional("XCONSOLE")]
        public static void WriteLine(object source, string format, params object[] args)
        {
#if XCONSOLE
            var config = GetConfig(source);
            Console.WriteLine(config.FormattedPrefix + format, args);
#endif
        }

        /// <summary>
        /// Writes a line at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="format">The line format.</param>
        /// <param name="args">The line arguments.</param>
        [Conditional("XCONSOLE")]
        public static void WriteLine(object source, int level, string format, params object[] args)
        {
#if XCONSOLE
            var config = GetConfig(source);
            if (config.Ignore(level)) return;
            Console.WriteLine(config.FormattedPrefix + format, args);
#endif
        }

        /// <summary>
        /// Writes a line.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="o">The object to write.</param>
        [Conditional("XCONSOLE")]
        public static void WriteLine(object source, object o)
        {
#if XCONSOLE
            var config = GetConfig(source);
            Console.WriteLine(config.FormattedPrefix + o);
#endif
        }

        /// <summary>
        /// Writes a line at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="o">The object to write.</param>
        [Conditional("XCONSOLE")]
        public static void WriteLine(object source, int level, object o)
        {
#if XCONSOLE
            var config = GetConfig(source);
            if (config.Ignore(level)) return;
            Console.WriteLine(config.FormattedPrefix + o);
#endif
        }

        /// <summary>
        /// Builds a block of lines at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="text">The lines.</param>
        /// <returns>The indented lines, if the level is not ignored; otherwise an empty string.</returns>
        public static string Lines(object source, int level, string text)
        {
#if XCONSOLE
            var info = GetConfig(source);
            if (info.Ignore(level)) return "";
            var prefix = new string(' ', info.FormattedPrefix.Length);
            text = "\n" + text;
            return text.Replace("\n", "\n" + prefix);
#else
            return "";
#endif
        }
    }
}