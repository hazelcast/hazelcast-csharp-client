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
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// A command line based Hazelcast <see cref="ConfigurationProvider"/>.
    /// </summary>
    /// <remarks>
    /// <para>Adds support for hazelcast.x.y arguments that do not respect the standard hazelcast:x:y pattern.</para>
    /// </remarks>
    internal class HazelcastCommandLineConfigurationProvider : CommandLineConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastCommandLineConfigurationProvider"/> class.
        /// </summary>
        public HazelcastCommandLineConfigurationProvider(HazelcastCommandLineConfigurationSource source)
            : base(FilterArgs(source.Args, source.SwitchMappings, source.KeyRoot), source.SwitchMappings)
        { }

        /// <summary>
        /// (internal for tests only)
        /// Filters arguments.
        /// </summary>
        internal static IEnumerable<string> FilterArgs(IEnumerable<string> args, IDictionary<string, string> switchMappings, string keyRoot)
        {
            var keyRootAndKeyDelimiter = keyRoot + ConfigurationPath.KeyDelimiter;
            var keyRootAndDot = keyRoot + '.';
            var slashKeyRootAndDot = '/' + keyRoot + '.';
            var dashKeyRootAndDot = "--" + keyRoot + '.';

            using var enumerator = args.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var arg = enumerator.Current;
                if (string.IsNullOrWhiteSpace(arg)) continue;

                int pos;

                if (switchMappings != null && arg.StartsWith("-", StringComparison.Ordinal))
                {
                    string argk, argv;
                    if ((pos = arg.IndexOf('=', StringComparison.Ordinal)) > 0)
                    {
                        argk = arg[..pos];
                        argv = arg[(pos + 1)..];
                    }
                    else
                    {
                        argk = arg;
                        argv = null;
                    }

                    argk = argk.Replace(".", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);

                    if (switchMappings.TryGetValue(argk, out var argm) && 
                        argm.StartsWith(keyRootAndKeyDelimiter, StringComparison.Ordinal))
                    {
                        // yield the key
                        yield return "--" + argm;

                        // yield the value
#pragma warning disable CA1508 // Avoid dead conditional code - false positive due to range operator?!
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (argv != null) yield return argv;
#pragma warning restore CA1508 
                        else if (enumerator.MoveNext()) yield return enumerator.Current;
                        continue; // next!
                    }
                }

                if (arg.StartsWith(slashKeyRootAndDot, StringComparison.Ordinal) ||
                    arg.StartsWith(dashKeyRootAndDot, StringComparison.Ordinal))
                {
                    if ((pos = arg.IndexOf('=', StringComparison.Ordinal)) > 0)
                    {
                        // yield the key
                        yield return arg[..pos].Replace(".", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);

                        // yield the value
                        yield return arg[(pos + 1)..];
                    }
                    else
                    {
                        // yield the key
                        yield return arg.Replace(".", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);

                        // yield the value
                        if (enumerator.MoveNext()) yield return enumerator.Current;
                    }
                }
                else if (arg.StartsWith(keyRootAndDot, StringComparison.Ordinal) &&
                         (pos = arg.IndexOf('=', StringComparison.Ordinal)) > 0)
                {
                    // yield the key
                    yield return "--" + arg[..pos].Replace(".", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);

                    // yield the value
                    yield return arg[(pos + 1)..];
                }

                // else ignore that arg (handled by the default command line provider)
            }
        }
    }
}
