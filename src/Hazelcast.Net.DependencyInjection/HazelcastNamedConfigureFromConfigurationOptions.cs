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

using Hazelcast.Configuration.Binding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Hazelcast.DependencyInjection
{
    /// <summary>
    /// Implementation of <see cref="IConfigureNamedOptions{TOptions}"/>.
    /// </summary>
    /// <remarks>
    /// <para>This class is a custom implementation of <see cref="NamedConfigureFromConfigurationOptions{TOptions}"/>
    /// which uses the Hazelcast configuration binder instead of the default Microsoft binder.</para>
    /// </remarks>
    public class HazelcastNamedConfigureFromConfigurationOptions : ConfigureNamedOptions<HazelcastOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastNamedConfigureFromConfigurationOptions"/> class.
        /// </summary>
        /// <param name="name">The name of the options.</param>
        /// <param name="configuration">The configuration.</param>
        public HazelcastNamedConfigureFromConfigurationOptions(string name, IConfiguration configuration)
            : base(name, configuration.HzBind)
        { }
    }
}
