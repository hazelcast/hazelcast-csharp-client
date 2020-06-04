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

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// A command line based Hazelcast <see cref="ConfigurationProvider"/>.
    /// </summary>
    internal class HazelcastCommandLineConfigurationProvider : CommandLineConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastCommandLineConfigurationProvider"/> class.
        /// </summary>
        /// <param name="args">The command line args.</param>
        /// <param name="switchMappings">The switch mappings.</param>
        public HazelcastCommandLineConfigurationProvider(IEnumerable<string> args, IDictionary<string, string> switchMappings = null)
            : base(FilterArgs(args), switchMappings)
        { }

        // internal for tests
        internal static IEnumerable<string> FilterArgs(IEnumerable<string> args)
        {
            using var enumerator = args.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var arg = enumerator.Current;
                if (string.IsNullOrWhiteSpace(arg)) continue;

                int pos;

                // must support
                // foo=bar
                // /foo bar
                // /foo=bar
                // --foo bar
                // --foo=bar
                // -foo bar
                // -foo=bar

                // note that in case of switch mapping, the ':' separator must be used,
                // as mapping will take place after we have replaced the dots

                if (arg.StartsWith("/hazelcast.") || arg.StartsWith("--hazelcast.") || arg.StartsWith("-hazelcast."))
                {
                    if ((pos = arg.IndexOf('=')) > 0)
                    {
                        // yield the key
                        yield return arg.Substring(0, pos).Replace(".", ConfigurationPath.KeyDelimiter);

                        // yield the value
                        yield return arg.Substring(pos + 1);
                    }
                    else
                    {
                        // yield the key
                        yield return arg.Replace(".", ConfigurationPath.KeyDelimiter);

                        // yield the value
                        if (enumerator.MoveNext())
                            yield return enumerator.Current;
                    }
                }
                else if (arg.StartsWith("hazelcast.") && (pos = arg.IndexOf('=')) > 0)
                {
                    // yield the key
                    yield return "--" + arg.Substring(0, pos).Replace(".", ConfigurationPath.KeyDelimiter);

                    // yield the value
                    yield return arg.Substring(pos + 1);
                }
                else
                {
                    // just pass the arg unchanged
                    yield return arg;
                }
            }
        }
    }
}
