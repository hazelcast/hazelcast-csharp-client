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
using Hazelcast.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Hazelcast
{
    /// <summary>
    /// Provides a way to build <see cref="HazelcastOptions"/> instances in a fluent way.
    /// </summary>
    public class HazelcastOptionsBuilder
    {
        private string[] _args;
        private IDictionary<string, string> _switchMappings;
        private Dictionary<string, string> _defaults;
        private Dictionary<string, string> _keyValues;
        private string _optionsFilePath;
        private string _optionsFileName;
        private string _environmentName;
        private string _altKey;
        private List<Action<IConfiguration, HazelcastOptions>> _configureActions;
        private List<Action<IConfigurationBuilder>> _setups;

        /// <summary>
        /// Sets the command-line arguments to use when building the options.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="switchMappings">Optional command-line switch mappings.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder With(string[] args, IDictionary<string, string> switchMappings = null)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _switchMappings = switchMappings;
            return this;
        }

        /// <summary>
        /// Adds a default key/value pair to use when building the options.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder WithDefault(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));
            // assuming value can be null or empty

            _defaults ??= new Dictionary<string, string>();
            _defaults[key] = value;
            return this;
        }

        /// <summary>
        /// Adds a key/value pair to use when building the options.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder With(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));
            // assuming value can be null or empty

            _keyValues ??= new Dictionary<string, string>();
            _keyValues[key] = value;
            return this;
        }

        /// <summary>
        /// Sets the path (without filename) to the options files to read when building the options.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>This options builder.</returns>
        /// <remarks>
        /// <para>By default, when not provided, the options file is searched in the
        /// default .NET configuration location, which usually is where the application resides.</para>
        /// </remarks>
        public HazelcastOptionsBuilder WithFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(filePath));

            _optionsFilePath = filePath;
            return this;
        }

        /// <summary>
        /// Sets the name (without path, with extension) of the options file to read when building the options.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>This options builder.</returns>
        /// <remarks>
        /// <para>By default, when not provided, the name is "hazelcast".</para>
        /// </remarks>
        public HazelcastOptionsBuilder WithFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(fileName));

            _optionsFileName = fileName;
            return this;
        }

        /// <summary>
        /// Sets the environment name to use when building the options.
        /// </summary>
        /// <param name="environmentName">The environment name.</param>
        /// <returns>This options builder.</returns>
        /// <remarks>
        /// <para>By default, when not provided, the name is determined the standard .NET way,
        /// i.e. from the <c>DOTNET_ENVIRONMENT</c> and <c>ASPNETCORE_ENVIRONMENT</c> variables and,
        /// if not specified, defaults to "Production".</para>
        /// </remarks>
        public HazelcastOptionsBuilder WithEnvironment(string environmentName)
        {
            if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(environmentName));

            _environmentName = environmentName;
            return this;
        }

        /// <summary>
        /// Sets the alternate key for options.
        /// </summary>
        /// <param name="key">The alternate key.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder WithAltKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));

            _altKey = key;
            return this;
        }

        /// <summary>
        /// Adds an <see cref="HazelcastOptions"/> configuration delegate.
        /// </summary>
        /// <param name="configure">The delegate.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder With(Action<IConfiguration, HazelcastOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            _configureActions ??= new List<Action<IConfiguration, HazelcastOptions>>();
            _configureActions.Add(configure);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="HazelcastOptions"/> configuration delegate.
        /// </summary>
        /// <param name="configure">The delegate.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder With(Action<HazelcastOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            return With((_, o) => configure(o));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationBuilder"/> configuration delegate.
        /// </summary>
        /// <param name="configure">The delegate.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder ConfigureBuilder(Action<IConfigurationBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            _setups ??= new List<Action<IConfigurationBuilder>>();
            _setups.Add(configure);
            return this;
        }

        /// <summary>
        /// Binds an additional options instance.
        /// </summary>
        /// <param name="key">The key for the instance.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder Bind(string key, object instance)
            => With((configuration, _) => configuration.HzBind(key, instance));

        private void Configure(IConfiguration configuration, HazelcastOptions options)
        {
            if (_configureActions == null) return;

            foreach (var configure in _configureActions)
                configure(configuration, options);
        }

        private void Setup(IConfigurationBuilder builder)
        {
            builder.AddHazelcastAndDefaults(_args, _switchMappings, _defaults, _keyValues, _optionsFilePath, _optionsFileName, _environmentName);

            if (_setups == null) return;

            foreach (var setup in _setups)
                setup(builder);
        }

        /// <summary>
        /// Builds the options.
        /// </summary>
        /// <returns>The options.</returns>
        public HazelcastOptions Build()
        {
            return HazelcastOptions.Build(Setup, Configure, _altKey);
        }
    }
}
