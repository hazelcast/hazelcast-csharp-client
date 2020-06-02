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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// An environment variables based Hazelcast <see cref="ConfigurationProvider"/>.
    /// </summary>
    internal class HazelcastEnvironmentVariablesConfigurationProvider : ConfigurationProvider
    {
        /// <inheritdoc />
        public override void Load()
            => Load(Environment.GetEnvironmentVariables());

        // internal for tests
        internal void Load(IDictionary envVariables)
        {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var envVariable in envVariables.Cast<DictionaryEntry>())
            {
                var key = (string) envVariable.Key;

                if (key.StartsWith("hazelcast.", StringComparison.OrdinalIgnoreCase))
                    key = key.Replace(".", ConfigurationPath.KeyDelimiter);

                Data[key] = (string) envVariable.Value;
            }
        }
    }
}