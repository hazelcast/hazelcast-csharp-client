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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// An environment variables based Hazelcast <see cref="ConfigurationProvider"/>.
    /// </summary>
    /// <remarks>
    /// <para>Adds support for the hazelcast.x.y variables that do not respect the standard hazelcast__x__y pattern.</para>
    /// </remarks>
    internal class HazelcastEnvironmentVariablesConfigurationProvider : ConfigurationProvider
    {
        private const string HazelcastAndDot = HazelcastOptions.SectionNameConstant + ".";
        private const string FailoverAndDot = HazelcastFailoverOptions.SectionNameConstant + ".";

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastEnvironmentVariablesConfigurationProvider"/>.
        /// </summary>
        public HazelcastEnvironmentVariablesConfigurationProvider(HazelcastEnvironmentVariablesConfigurationSource source)
        { }

        /// <inheritdoc />
        public override void Load()
            => Load(Environment.GetEnvironmentVariables());

        /// <summary>
        /// (internal for tests only)
        /// Loads environment variables.
        /// </summary>
        internal void Load(IDictionary envVariables)
        {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var envVariable in envVariables.Cast<DictionaryEntry>())
            {
                var key = (string) envVariable.Key;

                if (!key.StartsWith(HazelcastAndDot, StringComparison.OrdinalIgnoreCase) &&
                    !key.StartsWith(FailoverAndDot, StringComparison.OrdinalIgnoreCase)) continue;

                key = key.Replace(".", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
                Data[key] = (string)envVariable.Value;
            }
        }
    }
}
