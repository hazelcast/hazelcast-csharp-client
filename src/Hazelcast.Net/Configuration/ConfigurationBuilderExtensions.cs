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
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Provides extension methods for the <see cref="IConfigurationBuilder"/> interface.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration value from a file.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <returns></returns>
        public static IConfigurationBuilder AddHazelcastFile(this IConfigurationBuilder configurationBuilder, string filePath, string fileName, string environmentName)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));

            var fullpath = string.IsNullOrWhiteSpace(filePath)
                ? fileName
                : Path.Combine(filePath, fileName);

            var extension = Path.GetExtension(fullpath);
            var directory = Path.GetDirectoryName(fullpath);
            fullpath = Path.Combine(directory, Path.GetFileNameWithoutExtension(fullpath));

            // JSON files are reloadOnChange:false because we do not track configuration changes,
            // and we try to mitigate issues with running apps in Docker containers.
            // (see https://github.com/hazelcast/hazelcast-csharp-client/issues/322)

            configurationBuilder
                .AddJsonFile(fullpath + extension, true, false)
                .AddJsonFile(fullpath + '.' + DetermineEnvironment(environmentName) + extension, true, false);

            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration values from environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// <para>Adds support for the hazelcast.x.y variables that do not respect the standard hazelcast__x__y pattern.</para>
        /// <para>Does not add default support for environment variables.</para>
        /// </remarks>
        public static IConfigurationBuilder AddHazelcastEnvironmentVariables(this IConfigurationBuilder configurationBuilder)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));
            configurationBuilder.Add(new HazelcastEnvironmentVariablesConfigurationSource());
            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration values from the command line.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// <para>Adds support for `hazelcast.x.y` arguments that do not respect the standard `hazelcast:x:y` pattern.</para>
        /// <para>Does not add default support for command line arguments.</para>
        /// </remarks>
        public static IConfigurationBuilder AddHazelcastCommandLine(this IConfigurationBuilder configurationBuilder, string[] args)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));
            configurationBuilder.Add(new HazelcastCommandLineConfigurationSource { Args = args });
            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration values from an in-memory collection.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddHazelcastInMemoryCollection(this IConfigurationBuilder configurationBuilder, IEnumerable<KeyValuePair<string, string>> initialData)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));
            configurationBuilder.Add(new HazelcastMemoryConfigurationSource { InitialData = initialData });
            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration values from various sources.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The command line args.</param>
        /// <param name="keyValues">The optional key-value configuration pairs.</param>
        /// <param name="optionsFilePath">The optional path to the options file.</param>
        /// <param name="environmentName">An optional environment name.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// <para>Adds support for hazelcast-specific sources. Does not add default support for other sources.</para>
        /// <para>If <paramref name="environmentName"/> is missing, the environment name is determined using
        /// the <c>DOTNET_ENVIRONMENT</c> and <c>ASPNETCORE_ENVIRONMENT</c> environment variables. If not
        /// specified, the environment name is <c>Production</c>.</para>
        /// </remarks>
        public static IConfigurationBuilder AddHazelcast(this IConfigurationBuilder configurationBuilder, string[] args, IEnumerable<KeyValuePair<string, string>> keyValues = null, string optionsFilePath = null, string optionsFileName = null, string environmentName = null)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));

            if (string.IsNullOrWhiteSpace(optionsFileName))
                optionsFileName = "hazelcast.json";

            configurationBuilder
                .AddHazelcastFile(optionsFilePath, optionsFileName, environmentName);

            configurationBuilder
                .AddHazelcastEnvironmentVariables();

            if (args != null)
                configurationBuilder.AddHazelcastCommandLine(args);

            if (keyValues != null)
                configurationBuilder.AddHazelcastInMemoryCollection(keyValues);

            return configurationBuilder;
        }

        /// <summary>
        /// Adds the default <see cref="IConfigurationProvider"/> instances.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The command line args.</param>
        /// <param name="environmentName">An optional environment name.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// <para>Adds support for appsettings.json, environment variables and command line arguments. This is
        /// only useful in non-hosted environments where a configuration builder is created from scratch.</para>
        /// <para>If <paramref name="environmentName"/> is missing, the environment name is determined using
        /// the <c>DOTNET_ENVIRONMENT</c> and <c>ASPNETCORE_ENVIRONMENT</c> environment variables. If not
        /// specified, the environment name is <c>Production</c>.</para>
        /// </remarks>
        public static IConfigurationBuilder AddDefaults(this IConfigurationBuilder configurationBuilder, string[] args, string environmentName = null)
        {
            // JSON files are reloadOnChange:false because we do not track configuration changes,
            // and we try to mitigate issues with running apps in Docker containers.
            // (see https://github.com/hazelcast/hazelcast-csharp-client/issues/322)

            configurationBuilder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{DetermineEnvironment(environmentName)}.json", optional: true, reloadOnChange: false);

            configurationBuilder.AddEnvironmentVariables();

            if (args != null)
                configurationBuilder.AddCommandLine(args);

            return configurationBuilder;
        }

        /// <summary>
        /// Determines the runtime environment name.
        /// </summary>
        /// <param name="environmentName">An optional environment name.</param>
        /// <returns>The runtime environment name.</returns>
        /// <remarks>
        /// <para>Uses the rules specified at https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments
        /// i.e. uses the specified <paramref name="environmentName"/>, else the DOTNET_ENVIRONMENT environment
        /// variable, else the ASPNETCORE_ENVIRONMENT environment variable, else "Production".</para>
        /// </remarks>
        internal static string DetermineEnvironment(string environmentName)
        {
            if (!string.IsNullOrWhiteSpace(environmentName))
                return environmentName;

            var dotnet = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (!string.IsNullOrWhiteSpace(dotnet))
                return dotnet;

            var aspnetcore = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!string.IsNullOrWhiteSpace(aspnetcore))
                return aspnetcore;

            return "Production";
        }
    }
}
