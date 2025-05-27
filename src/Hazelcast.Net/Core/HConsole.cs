﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
#if HZ_CONSOLE
using System.Globalization;
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
#if HZ_CONSOLE_PUBLIC
    public
#else
    internal
#endif
    static class HConsole
    {
#if HZ_CONSOLE
        internal static readonly HConsoleOptions Options = new();
#endif

        // NOTES
        //
        // The HConsoleConfiguration class *must* exist regardless of the HZ_CONSOLE define, as
        // it is exposed by the methods below, so that the code can compile, and even though
        // these methods may not be compiled (see below). Nevertheless, to reduce its weight,
        // we can simplify the class body when HZ_CONSOLE is not defined.
        //
        // Methods below *must* exist regardless of the HZ_CONSOLE define, so that the code can
        // compile, but due to the [Conditional] attributes, their invocations will *not* be
        // compiled = zero cost. Nevertheless, to reduce their weight, we can simplify their
        // bodies when HZ_CONSOLE is not defined.
        //
        // The 'Lines' method cannot be marked [Conditional] because it returns a non-void
        // value. However, it should always be invoked from within a call to WriteLine, which
        // will *not* be compiled anyways.

        [Conditional("HZ_CONSOLE")]
        public static void Reset()
        {
#if HZ_CONSOLE
            Options.ClearAll().WithHConsoleWriter(null);
#endif
        }

        /// <summary>
        /// Configures.
        /// </summary>
        /// <param name="configure">An action to configure the options.</param>
        /// <remarks>
        /// <para>We HAVE to have this method wrapping an Action so that we can mark it Conditional. If
        /// we were to implement direct fluent configuration eg HConsole.Configure().SetLevel(...) that
        /// top-level Configure method could not be marked Conditional because it returns a value.</para>
        /// </remarks>
        [Conditional("HZ_CONSOLE")]
        public static void Configure(Action<HConsoleOptions> configure)
        {
#if HZ_CONSOLE
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            configure(Options);
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
        /// Writes a line and a stack trace.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="text">The text to write.</param>
        [Conditional("HZ_CONSOLE")]
        public static void TraceLine(object source, string text)
        {
#if HZ_CONSOLE
            WriteLine(source, 0, text + "\n" + Environment.StackTrace);
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
            var config = Options.GetOptions(source);
            if (config.Ignore(level)) return;
            Options.Writer.AppendLine(config.FormattedPrefix + text);
#endif
        }

        /// <summary>
        /// Writes a line and a stack trace at a level, if the level is not ignored.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="level">The level.</param>
        /// <param name="text">The text to write.</param>
        [Conditional("HZ_CONSOLE")]
        public static void TraceLine(object source, int level, string text)
        {
#if HZ_CONSOLE
            if (source == null) throw new ArgumentNullException(nameof(source));
            var config = Options.GetOptions(source);
            if (config.Ignore(level)) return;
            Options.Writer.AppendLine(config.FormattedPrefix + text + "\n" + Environment.StackTrace);
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
            var config = Options.GetOptions(source);
            if (config.Ignore(level)) return;
            Options.Writer.AppendLine(config.FormattedPrefix + o);
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
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var info = Options.GetOptions(source);
            if (info.Ignore(level)) return string.Empty;
            var prefix = new string(' ', info.FormattedPrefix.Length);
            text = "\n" + text;
            return text.Replace("\n", "\n" + prefix, StringComparison.Ordinal);
#else
            return "";
#endif
        }

        /// <summary>
        /// Gets the level of a source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The level for the source object.</returns>
        public static int Level(object source)
        {
#if HZ_CONSOLE
            if (source == null) throw new ArgumentNullException(nameof(source));
            var info = Options.GetOptions(source);
            return info.Level;
#else
            return 0;
#endif
        }
    }
}
