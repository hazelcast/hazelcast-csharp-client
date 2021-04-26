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

#if !HZ_CONSOLE
#pragma warning disable // too many 'unused args' etc
#endif

using System;
using System.Diagnostics;
using System.Text;
#if HZ_CONSOLE
using System.Globalization;
#endif

// About HConsole
//
// HConsole is a console / logging / debugging utility that is used for advanced tracing and troubleshooting. In
// addition, it supports heavy multi-threaded usage, whereas the default System.Console can become somewhat
// erratic in such situations.
//
// Every member of the HConsole class and its related configuration classes is marked [Conditional("HZ_CONSOLE")]
// which means that references to these members are NOT even compiled when the HZ_CONSOLE symbol is not defined.
//
// Therefore, HConsole has ZERO overhead on Release builds execution.
//
// In order for the code to compile, parts of the HConsole system (such as the class itself) MUST exist, regardless
// of the HZ_CONSOLE symbol. However, since no calls to the members will actually be compiled, their body can be
// stripped. Note that some members that return values cannot be marked [Conditional] - however, they are always
// invoked from within a [Conditional] member, which means they will practically never be invoked either.
//
// The entire HConsole system is internal. However, the HZ_CONSOLE_PUBLIC symbol can be defined at assembly level
// in order to publicly expose the system, which can be convenient for some troubleshooting tasks.

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides a console for troubleshooting.
    /// </summary>
#if HZ_CONSOLE_PUBLIC
    public
#else
    internal 
#endif
    static class HConsole
    {
#if HZ_CONSOLE
        internal static readonly HConsoleOptions Options = new HConsoleOptions();
        private static readonly StringBuilder TextBuilder = new StringBuilder();
        private static readonly object TextBuilderLock = new object();
#endif

        /// <summary>
        /// Gets the text.
        /// </summary>
        public static string Text
        {
#if HZ_CONSOLE
            get { lock (TextBuilderLock) return TextBuilder.ToString(); }
#else
            get => string.Empty;
#endif
        }

        /// <summary>
        /// Clears the text buffer.
        /// </summary>
        [Conditional("HZ_CONSOLE")]
        public static void Clear()
        {
#if HZ_CONSOLE
            lock (TextBuilderLock) TextBuilder.Clear();
#endif
        }

        /// <summary>
        /// Writes the text to the actual System.Console and clears the buffer.
        /// </summary>
        [Conditional("HZ_CONSOLE")]
        public static void WriteAndClear()
        {
#if HZ_CONSOLE
            lock (TextBuilderLock)
            {
                if (TextBuilder.Length > 0)
                {
                    Console.Write(TextBuilder.ToString());
                    TextBuilder.Clear();
                }
            }
#endif
        }

        /// <summary>
        /// Gets a disposable that, when disposed, will write and clear the console.
        /// </summary>
        [Conditional("HZ_CONSOLE")]
        public static void Reset()
        {
#if HZ_CONSOLE
            Options.ClearAll();
            lock (TextBuilderLock) TextBuilder.Clear();
#endif
        }

        /// <summary>
        /// Configures.
        /// </summary>
        /// <param name="configure">An action to configure the options.</param>
        [Conditional("HZ_CONSOLE")]
        public static void Configure(Action<HConsoleOptions> configure)
        {
#if HZ_CONSOLE
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            configure(Options);
#endif
        }

        /// <summary>
        /// Gets a disposable that, when disposed, will write and clear the console.
        /// </summary>
        /// <param name="configure">An optional action to configure the options.</param>
        /// <returns>A disposable that, when disposed, will write and clear the console.</returns>
        // cannot be [Conditional] when return type is not void
        public static IDisposable Capture(Action<HConsoleOptions> configure = null)
        {
#if HZ_CONSOLE
            configure?.Invoke(Options);
            return new Disposable();
#else
            return null;
#endif
        }

#if HZ_CONSOLE
        private class Disposable : IDisposable
        {
            public void Dispose()
            {
                WriteAndClear();
            }
        }
#endif

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
            var config = Options.Get(source);
            if (config.Ignore(level)) return;
            lock (TextBuilderLock) TextBuilder.AppendLine(config.FormattedPrefix + text);
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
            var config = Options.Get(source);
            if (config.Ignore(level)) return;
            lock (TextBuilderLock) TextBuilder.AppendLine(config.FormattedPrefix + text + "\n" + Environment.StackTrace);
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
            var config = Options.Get(source);
            if (config.Ignore(level)) return;
            lock (TextBuilderLock) TextBuilder.AppendFormat(CultureInfo.InvariantCulture, config.FormattedPrefix + format + Environment.NewLine, args);
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
            var config = Options.Get(source);
            if (config.Ignore(level)) return;
            lock (TextBuilderLock) TextBuilder.AppendLine(config.FormattedPrefix + o);
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
            var info = Options.Get(source);
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
