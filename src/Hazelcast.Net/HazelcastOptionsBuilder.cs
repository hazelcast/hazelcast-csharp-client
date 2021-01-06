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
        private Dictionary<string, string> _keyValues;
        private string _optionsFilePath;
        private string _optionsFileName;
        private string _environmentName;
        private List<Action<IConfiguration, HazelcastOptions>> _configureActions;

        /// <summary>
        /// Sets the command-line arguments to use when building the options.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>This options builder.</returns>
        public HazelcastOptionsBuilder With(string[] args)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));
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
        /// <para>By default, when not provided, the options file is searched in searched in the
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

        private void ConfigureActions(IConfiguration configuration, HazelcastOptions options)
        {
            if (_configureActions == null) return;

            foreach (var configure in _configureActions)
                configure(configuration, options);
        }

        /// <summary>
        /// Builds the options.
        /// </summary>
        /// <returns>The options.</returns>
        public HazelcastOptions Build()
        {
            return HazelcastOptions.Build(_args, _keyValues, _optionsFilePath, _optionsFileName, _environmentName, ConfigureActions);
        }
    }
}
