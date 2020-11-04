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
        /// <param name="optionsFilePath">Optional options file path (without filename).</param>
        /// <param name="optionsFileName">Optional options file name (without path, with extension).</param>
        /// <param name="environmentName">Optional environment name.</param>
        /// <param name="configure">Optional <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <returns>Hazelcast options.</returns>
        public static HazelcastOptions Build(string[] args = null, IEnumerable<KeyValuePair<string, string>> keyValues = null, string optionsFilePath = null, string optionsFileName = null, string environmentName = null, Action<IConfiguration, HazelcastOptions> configure = null)
        {
            return Build(builder =>
            {
                builder.AddDefaults(args, environmentName);
                builder.AddHazelcast(args, keyValues, optionsFilePath, optionsFileName, environmentName);
            }, configure);
        }

        /// <summary>
        /// Builds Hazelcast options.
        /// </summary>
        /// <param name="setup">An <see cref="IConfigurationBuilder"/> setup delegate.</param>
        /// <param name="configure">Optional <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <returns>Hazelcast options.</returns>
        public static HazelcastOptions Build(Action<IConfigurationBuilder> setup, Action<IConfiguration, HazelcastOptions> configure = null)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var builder = new ConfigurationBuilder();
            setup(builder);
            var configuration = builder.Build();

            return Build(configuration, configure);
        }

        /// <summary>
        /// (internal for tests only)
        /// Builds Hazelcast options.
        /// </summary>
        internal static HazelcastOptions Build(Action<IConfigurationBuilder> setup, Action<IConfiguration, HazelcastOptions> configure, string altKey)
        {
            // build hazelcast options, optionally binding alternate keys for tests
            // this allows 1 json file to contain several configuration sets, and that
            // is convenient when using the "user secrets" during tests.

            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var builder = new ConfigurationBuilder();
            setup(builder);
            var configuration = builder.Build();

            return Build(configuration, configure, altKey);
        }

        /// <summary>
        /// Builds Hazelcast options.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="configure">Optional <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <returns>Hazelcast options.</returns>
        public static HazelcastOptions Build(IConfiguration configuration, Action<IConfiguration, HazelcastOptions> configure = null)
            => Build(configuration, configure, null);

        // build options, optionally binding alternate keys
        private static HazelcastOptions Build(IConfiguration configuration, Action<IConfiguration, HazelcastOptions> configure, string altKey)
        {
            // must HzBind here and not simply Bind because we use our custom
            // binder which handles more situations such as ignoring and/or
            // renaming properties

            var options = new HazelcastOptions();
            configuration.HzBind(Hazelcast, options);

            if (altKey != null && altKey != Hazelcast)
                configuration.HzBind(altKey, options);

            configure?.Invoke(configuration, options);
            return options;
        }
    }
}
