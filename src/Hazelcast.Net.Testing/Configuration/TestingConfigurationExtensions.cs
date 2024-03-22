// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Hazelcast.Testing.Configuration
{
    /// <summary>
    /// Provides extension methods for configuration.
    /// </summary>
    public static class TestingConfigurationExtensions
    {
        /// <summary>
        /// Sets user secrets.
        /// </summary>
        /// <param name="builder">The options builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="key">The key.</param>
        /// <param name="optional">Whether secrets are optional.</param>
        /// <returns>This options builder.</returns>
        public static HazelcastOptionsBuilder WithUserSecrets(this HazelcastOptionsBuilder builder, Assembly assembly, string key = HazelcastOptions.SectionNameConstant, bool optional = true)
        {
            return builder
                .WithAltKey(key)
                .ConfigureBuilder(configurationBuilder =>
                {
                    configurationBuilder.AddUserSecrets(assembly, optional);
                });
        }
    }
}
