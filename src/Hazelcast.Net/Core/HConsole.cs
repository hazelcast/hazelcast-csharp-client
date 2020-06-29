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

#if !HZ_CONSOLE
#pragma warning disable // too many 'unused args' etc
#endif

using System;
using System.Diagnostics;
#if HZ_CONSOLE
using System.Collections.Generic;

#endif

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides a console for troubleshooting.
    /// </summary>
    /// <remarks>
    /// <para>To enable the console, define HZ_CONSOLE. Otherwise, none of the console
    /// method invocations will be compiled (thanks to <see cref="ConditionalAttribute"/>)
    /// and therefore the impact on actual (production) code will be null.</para>
    /// </remarks>
    internal static class HConsole
    {
#if HZ_CONSOLE
        private static readonly Dictionary<object, HConsoleConfiguration> SourceConfigs = new Dictionary<object, HConsoleConfiguration>();
        private static readonly Dictionary<Type, HConsoleConfiguration> TypeConfigs = new Dictionary<Type, HConsoleConfiguration>();

        static HConsole()
        {
            // setup default configuration
            Configure<object>(config =>
            {
                config.SetMaxLevel(-1);
                config.SetIndent(0);
                config.SetPrefix("");
            });
        }

        private static HConsoleConfiguration GetConfig(object source)
        {
            if (!SourceConfigs.TryGetValue(source, out var config)) config = new HConsoleConfiguration();

            var type = source.GetType();
            while (type != null && !config.IsComplete)
            {
                if (TypeConfigs.TryGetValue(type, out var c)) config = config.Merge(c);
                type = type.BaseType;
            }

            return config;
        }
#endif

        // NOTES
        //
        // The HConsoleConfiguration class *must* exist regardless of the HZ_CONSOLE define, as
        // it is exposed by the methods below, so that the code can compile,
        // and even though these methods may not be compiled (see below).
        // However, to reduce its weight, we can simplify the class body when
        // HZ_CONSOLE is not defined.
        //
        // Methods below *must* exist regardless of the HZ_CONSOLE define, so that
        // the code can compile, but due to the [Conditional] attributes, their
        // invocations will *not* be compiled = zero cost.
        // Nevertheless, to reduce their weight, we can simplify their bodies
        // when HZ_CONSOLE is not defined.
        // The 'Lines' method cannot be marked [Conditional] because it returns
        // a non-void value. However, it should always be invoked from within
        // a call to WriteLine, which will *not* be compiled anyways.

        /// <summary>
        /// Configure the console for a source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="configure">A action to configure the console.</param>
        [Conditional("HZ_CONSOLE")]
        public static void Configure(object source, Action<HConsoleConfiguration> configure)
        {
#if HZ_CONSOLE
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            if (!SourceConfigs.TryGetValue(source, out var info))
                info = SourceConfigs[source] = new HConsoleConfiguration();
            configure(info);
#endif
        }

        /// <summary>
        /// Configure the console for a type of objects.
        /// </summary>
        /// <typeparam name="TObject">The type of objects.</typeparam>
        /// <param name="configure">A action to configure the console.</param>
        [Conditional("HZ_CONSOLE")]
        public static void Configure<TObject>(Action<HConsoleConfiguration> configure)
        {
#if HZ_CONSOLE
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var type = typeof(TObject);
            if (!TypeConfigs.TryGetValue(type, out var info))
                info = TypeConfigs[type] = new HConsoleConfiguration();
            configure(info);
#endif
        }

        /// <summary>
        /// Writes a line.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="text">The text to write.</param>
        [Conditional("HZ_CONSOLE")]
        public static void WriteLine(object source, string text)
        {
#if HZ_CONSOLE
            WriteLine(source, 0, text);
#endif
        }

        /// <summary>
        /// Writes a line at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="text">The text to write.</param>
        [Conditional("HZ_CONSOLE")]
        public static void WriteLine(object source, int level, string text)
        {
#if HZ_CONSOLE
            if (source == null) throw new ArgumentNullException(nameof(source));
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
        [Conditional("HZ_CONSOLE")]
        public static void WriteLine(object source, string format, params object[] args)
        {
#if HZ_CONSOLE
            WriteLine(source, 0, format, args);
#endif
        }

        /// <summary>
        /// Writes a line at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="format">The line format.</param>
        /// <param name="args">The line arguments.</param>
        [Conditional("HZ_CONSOLE")]
        public static void WriteLine(object source, int level, string format, params object[] args)
        {
#if HZ_CONSOLE
            if (source == null) throw new ArgumentNullException(nameof(source));
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
        [Conditional("HZ_CONSOLE")]
        public static void WriteLine(object source, object o)
        {
#if HZ_CONSOLE
            WriteLine(source, 0, o);
#endif
        }

        /// <summary>
        /// Writes a line at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="o">The object to write.</param>
        [Conditional("HZ_CONSOLE")]
        public static void WriteLine(object source, int level, object o)
        {
#if HZ_CONSOLE
            if (source == null) throw new ArgumentNullException(nameof(source));
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
#if HZ_CONSOLE
            if (source == null) throw new ArgumentNullException(nameof(source));
            var info = GetConfig(source);
            if (info.Ignore(level)) return "";
            var prefix = new string(' ', info.FormattedPrefix.Length);
            text = "\n" + text;
            return text.Replace("\n", "\n" + prefix, StringComparison.Ordinal);
#else
            return "";
#endif
        }
    }
}
