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
using System.Runtime.CompilerServices;
using System.Threading;

#if XCONSOLE

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
        private static readonly ConditionalWeakTable<object, SourceInfo> _prefixes
            = new ConditionalWeakTable<object, SourceInfo>();

        private sealed class SourceInfo
        {
            public SourceInfo(int indent, string prefix)
            {
                Indent = indent;
                Prefix = prefix;
            }

            public static readonly SourceInfo Default = new SourceInfo(0, "");

            public int Indent { get; }

            public string Prefix { get; }
        }
#endif

        /// <summary>
        /// Setup the console for a source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="indent">The indentation.</param>
        /// <param name="prefix">The prefix.</param>
        [Conditional("XCONSOLE")]
        public static void Setup(object source, int indent, string prefix)
        {
#if XCONSOLE
            // this is netstandard 2.1
            //_prefixes.AddOrUpdate(source, new SourceInfo(indent, " " + prefix));

            _prefixes.Add(source, new SourceInfo(indent, " " + prefix));
#endif
        }

#if XCONSOLE
        private static string GetPrefix(object source)
        {
            if (!_prefixes.TryGetValue(source, out var info))
                info = SourceInfo.Default;

            return $"{new string(' ', info.Indent)}[{Thread.CurrentThread.ManagedThreadId:00}]{info.Prefix}: ";
        }
#endif

        /// <summary>
        /// Writes a line.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="text">The text to write.</param>
        [Conditional("XCONSOLE")]
        public static void WriteLine(object source, string text)
        {
#if XCONSOLE
            Console.WriteLine(GetPrefix(source) + text);
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
            Console.WriteLine(GetPrefix(source) + format, args);
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
            Console.WriteLine(GetPrefix(source) + o);
#endif
        }
    }
}