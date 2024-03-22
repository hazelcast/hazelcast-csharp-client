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

using System;
using System.Collections.Generic;
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    /// <summary>
    /// Provides a base class for both <see cref="HazelcastOptionsBuilder"/> and <see cref="HazelcastFailoverOptionsBuilder"/>.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TBuilder"></typeparam>
    public abstract class HazelcastOptionsBuilderBase<TOptions, TBuilder>
        where TOptions : HazelcastOptionsBase, new()
    {
        private string[] _args;
        private IDictionary<string, string> _switchMappings;
        private Dictionary<string, string> _defaults;
        private Dictionary<string, string> _keyValues;
        private string _optionsFilePath;
        private string _optionsFileName;
        private string _environmentName;
        private string _altKey;
        private List<Action<IConfiguration, TOptions>> _configureActions;
        private List<Action<TOptions>> _preConfigureActions;
        private List<Action<IConfigurationBuilder>> _setups;
        private IServiceProvider _serviceProvider;
        private IConfiguration _configuration;

        /// <summary>
        /// Gets this instance as <typeparamref name="TBuilder"/>.
        /// </summary>
        protected abstract TBuilder ThisBuilder { get; }

        /// <summary>
        /// Sets the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>This options builder.</returns>
        internal TBuilder With(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return ThisBuilder;
        }

        /// <summary>
        /// Sets the command-line arguments to use when building the options.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="switchMappings">Optional command-line switch mappings.</param>
        /// <returns>This options builder.</returns>
        public TBuilder With(string[] args, IDictionary<string, string> switchMappings = null)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _switchMappings = switchMappings;
            return ThisBuilder;
        }

        /// <summary>
        /// Adds a default key/value pair to use when building the options.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>This options builder.</returns>
        public TBuilder WithDefault(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));
            // assuming value can be null or empty

            _defaults ??= new Dictionary<string, string>();
            _defaults[key] = value;
            return ThisBuilder;
        }

        /// <summary>
        /// Adds a default key/value pair to use when building the options.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>This options builder.</returns>
        public TBuilder WithDefault(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));
            // assuming value can be null or empty

            _defaults ??= new Dictionary<string, string>();
            _defaults[key] = value.ToString();
            return ThisBuilder;
        }

        /// <summary>
        /// Adds a key/value pair to use when building the options.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>This options builder.</returns>
        public TBuilder With(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));
            // assuming value can be null or empty

            _keyValues ??= new Dictionary<string, string>();
            _keyValues[key] = value;
            return ThisBuilder;
        }

        /// <summary>
        /// Adds a key/value pair to use when building the options.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>This options builder.</returns>
        public TBuilder With(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));
            // assuming value can be null or empty

            _keyValues ??= new Dictionary<string, string>();
            _keyValues[key] = value.ToString();
            return ThisBuilder;
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
        public TBuilder WithFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(filePath));

            _optionsFilePath = filePath;
            return ThisBuilder;
        }

        /// <summary>
        /// Sets the name (without path, with extension) of the options file to read when building the options.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>This options builder.</returns>
        /// <remarks>
        /// <para>By default, when not provided, the name is "hazelcast".</para>
        /// </remarks>
        public TBuilder WithFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(fileName));

            _optionsFileName = fileName;
            return ThisBuilder;
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
        public TBuilder WithEnvironment(string environmentName)
        {
            if (string.IsNullOrWhiteSpace(environmentName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(environmentName));

            _environmentName = environmentName;
            return ThisBuilder;
        }

        /// <summary>
        /// Sets the alternate key for options.
        /// </summary>
        /// <param name="key">The alternate key.</param>
        /// <returns>This options builder.</returns>
        public TBuilder WithAltKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(key));

            _altKey = key;
            return ThisBuilder;
        }

        /// <summary>
        /// Adds an <typeparamref name="TOptions"/> configuration delegate.
        /// </summary>
        /// <param name="configure">The delegate.</param>
        /// <returns>This options builder.</returns>
        public TBuilder With(Action<IConfiguration, TOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            _configureActions ??= new List<Action<IConfiguration, TOptions>>();
            _configureActions.Add(configure);
            return ThisBuilder;
        }

        /// <summary>
        /// Adds an <typeparamref name="TOptions"/> default configuration delegate.
        /// </summary>
        /// <param name="configure">The delegate.</param>
        /// <returns>This options builder.</returns>
        public TBuilder WithDefault(Action<TOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            _preConfigureActions ??= new List<Action<TOptions>>();
            _preConfigureActions.Add(configure);
            return ThisBuilder;
        }

        /// <summary>
        /// Adds an <typeparamref name="TOptions"/> configuration delegate.
        /// </summary>
        /// <param name="configure">The delegate.</param>
        /// <returns>This options builder.</returns>
        public TBuilder With(Action<TOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            return With((_, o) => configure(o));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationBuilder"/> configuration delegate.
        /// </summary>
        /// <param name="configure">The delegate.</param>
        /// <returns>This options builder.</returns>
        /// <remarks>Do not specify configuration delegates if an <see cref="IConfiguration"/>
        /// is specified via <see cref="AddConfiguration"/>.</remarks>
        public TBuilder ConfigureBuilder(Action<IConfigurationBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            _setups ??= new List<Action<IConfigurationBuilder>>();
            _setups.Add(configure);
            return ThisBuilder;
        }

        /// <summary>
        /// Sets the <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>This options builder.</returns>
        /// <remarks>When an <see cref="IConfiguration"/> is added, no <see cref="IConfigurationBuilder"/>
        /// configuration delegate should be registered via <see cref="ConfigureBuilder"/>.</remarks>
        public TBuilder AddConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            return ThisBuilder;
        }

        /// <summary>
        /// Binds an additional options instance.
        /// </summary>
        /// <param name="key">The key for the instance.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>This options builder.</returns>
        public TBuilder Bind(string key, object instance)
            => With((configuration, _) => configuration.HzBind(key, instance));

        private void PreConfigure(TOptions options)
        {
            if (_preConfigureActions == null) return;

            foreach (var preConfigure in _preConfigureActions)
                preConfigure(options);
        }

        private void Configure(IConfiguration configuration, TOptions options)
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
        public TOptions Build()
        {
            if (_configuration == null) return Build(Setup, PreConfigure, Configure, _altKey);
            if (_setups is { Count: > 0 })
                throw new ConfigurationException("It is illegal to provide an IConfiguration object as well as configuration delegates.");
            return Build(_configuration, PreConfigure, Configure, _altKey);
        }

        /// <summary>
        /// (internal for tests only) Builds Hazelcast options.
        /// </summary>
        /// <param name="setup">An <see cref="IConfigurationBuilder"/> setup delegate.</param>
        /// <param name="preConfigure">An <typeparamref name="TOptions"/> pre-configuration delegate.</param>
        /// <param name="configure">Optional <typeparamref name="TOptions"/> configuration delegate.</param>
        /// <returns>Hazelcast options.</returns>
        internal TOptions Build(Action<IConfigurationBuilder> setup, Action<TOptions> preConfigure = null, Action<IConfiguration, TOptions> configure = null)
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
        /// <param name="preConfigure">A <typeparamref name="TOptions"/> pre-configuration delegate.</param>
        /// <param name="configure">A <typeparamref name="TOptions"/> configuration delegate.</param>
        /// <param name="altKey">An alternate key.</param>
        /// <returns>Hazelcast options.</returns>
        /// <remarks>
        /// <para>This is used in tests only and not meant to be public. If <paramref name="altKey"/> is not
        /// <c>null</c>, options starting with that key will bind after those starting with "hazelcast" and
        /// override them. This allows one json file to contain several configuration sets, which is
        /// convenient for instance when using the "user secrets" during tests.</para>
        /// </remarks>
        internal TOptions Build(Action<IConfigurationBuilder> setup, Action<TOptions> preConfigure, Action<IConfiguration, TOptions> configure, string altKey)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));

            var builder = new ConfigurationBuilder();
            setup(builder);
            var configuration = builder.Build();

            return Build(configuration, preConfigure, configure, altKey);
        }

        // builds options, no alternate keys
        private TOptions Build(IConfiguration configuration, Action<TOptions> preConfigure, Action<IConfiguration, TOptions> configure = null)
            => Build(configuration, preConfigure, configure, null);

        // builds options, optionally binding alternate keys
        private TOptions Build(IConfiguration configuration, Action<TOptions> preConfigure, Action<IConfiguration, TOptions> configure, string altKey)
        {
            // must HzBind here and not simply Bind because we use our custom
            // binder which handles more situations such as ignoring and/or
            // renaming properties

            var options = new TOptions { ServiceProvider = _serviceProvider };

            preConfigure?.Invoke(options);

            var sectionName = options.SectionName;

            configuration.HzBind(sectionName, options);

            if (altKey != null && altKey != sectionName)
                configuration.HzBind(altKey, options);

            configure?.Invoke(configuration, options);
            return options;
        }
    }
}
