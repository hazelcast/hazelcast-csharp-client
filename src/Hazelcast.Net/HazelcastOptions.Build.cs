﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Configuration.Binding;
using Microsoft.Extensions.Configuration;

namespace Hazelcast
{
    public partial class HazelcastOptions // Build
    {
        /// <summary>
        /// Gets the Hazelcast configuration section name, which is <c>"hazelcast"</c>.
        /// </summary>
        /// <returns>The Hazelcast configuration section name, which is <c>"hazelcast"</c>.</returns>
        internal const string Hazelcast = "hazelcast";

        /// <summary>
        /// (internal for tests only) Builds Hazelcast options.
        /// </summary>
        /// <param name="setup">An <see cref="IConfigurationBuilder"/> setup delegate.</param>
        /// <param name="preConfigure">An <see cref="HazelcastOptions"/> pre-configuration delegate.</param>
        /// <param name="configure">Optional <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <returns>Hazelcast options.</returns>
        internal static HazelcastOptions Build(Action<IConfigurationBuilder> setup, Action<HazelcastOptions> preConfigure = null, Action<IConfiguration, HazelcastOptions> configure = null)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var builder = new ConfigurationBuilder();
            setup(builder);
            var configuration = builder.Build();

            return Build(configuration, preConfigure, configure);
        }

        /// <summary>
        /// (internal for tests only) Builds Hazelcast options, using an alternate key.
        /// </summary>
        /// <param name="setup">An <see cref="IConfigurationBuilder"/> setup delegate.</param>
        /// <param name="preConfigure">A <see cref="HazelcastOptions"/> pre-configuration delegate.</param>
        /// <param name="configure">A <see cref="HazelcastOptions"/> configuration delegate.</param>
        /// <param name="altKey">An alternate key.</param>
        /// <returns>Hazelcast options.</returns>
        /// <remarks>
        /// <para>This is used in tests only and not meant to be public. If <paramref name="altKey"/> is not
        /// <c>null</c>, options starting with that key will bind after those starting with "hazelcast" and
        /// override them. This allows one json file to contain several configuration sets, which is
        /// convenient for instance when using the "user secrets" during tests.</para>
        /// </remarks>
        internal static HazelcastOptions Build(Action<IConfigurationBuilder> setup, Action<HazelcastOptions> preConfigure, Action<IConfiguration, HazelcastOptions> configure, string altKey)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var builder = new ConfigurationBuilder();
            setup(builder);
            var configuration = builder.Build();

            return Build(configuration, preConfigure, configure, altKey);
        }

        // builds options, no alternate keys
        private static HazelcastOptions Build(IConfiguration configuration, Action<HazelcastOptions> preConfigure, Action<IConfiguration, HazelcastOptions> configure = null)
            => Build(configuration, preConfigure, configure, null);

        // builds options, optionally binding alternate keys
        private static HazelcastOptions Build(IConfiguration configuration, Action<HazelcastOptions> preConfigure, Action<IConfiguration, HazelcastOptions> configure, string altKey)
        {
            // must HzBind here and not simply Bind because we use our custom
            // binder which handles more situations such as ignoring and/or
            // renaming properties

            var options = new HazelcastOptions();

            preConfigure?.Invoke(options);

            configuration.HzBind(Hazelcast, options);

            if (altKey != null && altKey != Hazelcast)
                configuration.HzBind(altKey, options);

            configure?.Invoke(configuration, options);
            return options;
        }
    }
}
