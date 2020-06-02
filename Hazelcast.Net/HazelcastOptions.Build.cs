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
using System.Collections.Generic;
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Microsoft.Extensions.Configuration;

namespace Hazelcast
{
    public partial class HazelcastOptions // Build
    {
        public const string Hazelcast = "hazelcast";

        /// <summary>
        /// Builds Hazelcast options.
        /// </summary>
        /// <param name="args">Optional command-line arguments.</param>
        /// <param name="keyValues">Optional key-value pairs.</param>
        /// <param name="optionsFilePath">The options file path (without filename).</param>
        /// <param name="optionsFileName">The options file name (without path).</param>
        /// <param name="environmentName">Optional environment name.</param>
        /// <param name="configureOptions">Optional action to further configure the options.</param>
        /// <returns>Hazelcast options.</returns>
        public static HazelcastOptions Build(string[] args = null, IEnumerable<KeyValuePair<string, string>> keyValues = null, string optionsFilePath = null, string optionsFileName = null, string environmentName = null, Action<IConfiguration, HazelcastOptions> configureOptions = null)
        {
            return Build(builder =>
            {
                builder.AddHazelcast(args, keyValues, optionsFilePath, optionsFileName, environmentName);
            }, configureOptions);
        }

        /// <summary>
        /// Builds Hazelcast options.
        /// </summary>
        /// <param name="setup">An <see cref="IConfigurationBuilder"/> setup action.</param>
        /// <param name="configureOptions">Optional action to further configure the options.</param>
        /// <returns>Hazelcast options.</returns>
        public static HazelcastOptions Build(Action<IConfigurationBuilder> setup, Action<IConfiguration, HazelcastOptions> configureOptions = null)
        {
            var builder = new ConfigurationBuilder();
            setup(builder);
            var configuration = builder.Build();

            return Build(configuration, configureOptions);
        }

        /// <summary>
        /// Builds Hazelcast options.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="configureOptions">Optional action to further configure the options.</param>
        /// <returns>Hazelcast options.</returns>
        public static HazelcastOptions Build(IConfiguration configuration, Action<IConfiguration, HazelcastOptions> configureOptions = null)
        {
            // must HzBind here and not simply Bind because we use our custom
            // binder which handles more situations such as ignoring and/or
            // renaming properties

            var options = new HazelcastOptions();
            configuration.HzBind(Hazelcast, options);
            configureOptions?.Invoke(configuration, options);
            return options;
        }
    }
}