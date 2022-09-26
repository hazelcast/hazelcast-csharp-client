// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;

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
        /// <param name="environmentName">The name of the environment.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <returns></returns>
        public static IConfigurationBuilder AddHazelcastFile(this IConfigurationBuilder configurationBuilder, string filePath, string fileName, string environmentName)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));

            var (fullpath, extension) = GetHazelcastFilePath(filePath, fileName);

            // JSON files are reloadOnChange:false because we do not track configuration changes,
            // and we try to mitigate issues with running apps in Docker containers.
            // (see https://github.com/hazelcast/hazelcast-csharp-client/issues/322)

            configurationBuilder
                .AddJsonFile(fullpath + extension, true, false)
                .AddJsonFile(fullpath + '.' + DetermineEnvironment(environmentName) + extension, true, false);

            return configurationBuilder;
        }

        private static (string, string) GetHazelcastFilePath(string filePath, string fileName)
        {
            var fullpath = string.IsNullOrWhiteSpace(filePath)
                ? fileName
                : Path.Combine(filePath, fileName);

            var extension = Path.GetExtension(fullpath);
            var directory = Path.GetDirectoryName(fullpath) ?? "";
            fullpath = Path.Combine(directory, Path.GetFileNameWithoutExtension(fullpath));
            return (fullpath, extension);
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
        /// <param name="args">The command line args.</param>
        /// <param name="switchMappings">Command line switch mappings.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// <para>Adds support for `hazelcast.x.y` arguments that do not respect the standard `hazelcast:x:y` pattern.</para>
        /// <para>Does not add default support for command line arguments.</para>
        /// </remarks>
        public static IConfigurationBuilder AddHazelcastCommandLine(this IConfigurationBuilder configurationBuilder, string[] args, IDictionary<string, string> switchMappings = null)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));
            configurationBuilder.Add(new HazelcastCommandLineConfigurationSource { Args = args, SwitchMappings = switchMappings });
            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration values from an in-memory collection.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="initialData">The initial key value configuration pairs.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddHazelcastInMemoryCollection(this IConfigurationBuilder configurationBuilder, IEnumerable<KeyValuePair<string, string>> initialData)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));
            configurationBuilder.Add(new HazelcastMemoryConfigurationSource { InitialData = initialData });
            return configurationBuilder;
        }

        /// <summary>
        /// Configures an <see cref="IConfigurationBuilder"/> to read default and Hazelcast configuration values from various sources.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The command line args.</param>
        /// <param name="switchMappings">Command line switch mappings.</param>
        /// <param name="defaults">The defaults key-value configuration pairs.</param>
        /// <param name="keyValues">The optional key-value configuration pairs.</param>
        /// <param name="optionsFilePath">The optional path to the options file.</param>
        /// <param name="optionsFileName">The optional name of the options file.</param>
        /// <param name="environmentName">An optional environment name.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// <para>Adds support for default and hazelcast-specific sources.</para>
        /// <para>If <paramref name="environmentName"/> is missing, the environment name is determined using
        /// the <c>DOTNET_ENVIRONMENT</c> and <c>ASPNETCORE_ENVIRONMENT</c> environment variables. If not
        /// specified, the environment name is <c>Production</c>.</para>
        /// </remarks>
        public static IConfigurationBuilder AddHazelcastAndDefaults(this IConfigurationBuilder configurationBuilder,
            string[] args,
            IDictionary<string, string> switchMappings = null,
            IEnumerable<KeyValuePair<string, string>> defaults = null,
            IEnumerable<KeyValuePair<string, string>> keyValues = null,
            string optionsFilePath = null,
            string optionsFileName = null,
            string environmentName = null)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));

            if (defaults != null)
                configurationBuilder.AddHazelcastInMemoryCollection(defaults); // handles both standard and hazelcast syntax

            // JSON files are reloadOnChange:false because we do not track configuration changes,
            // and we try to mitigate issues with running apps in Docker containers.
            // (see https://github.com/hazelcast/hazelcast-csharp-client/issues/322)

            if (string.IsNullOrWhiteSpace(optionsFileName))
                optionsFileName = "hazelcast.json";

            configurationBuilder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{DetermineEnvironment(environmentName)}.json", optional: true, reloadOnChange: false)
                .AddHazelcastFile(optionsFilePath, optionsFileName, environmentName);

            configurationBuilder
                .AddEnvironmentVariables()
                .AddHazelcastEnvironmentVariables();

            if (args != null)
                configurationBuilder
                    .AddCommandLine(args, switchMappings)
                    .AddHazelcastCommandLine(args, switchMappings);

            if (keyValues != null)
                configurationBuilder.AddHazelcastInMemoryCollection(keyValues); // handles both standard and hazelcast syntax

            return configurationBuilder;
        }

        /// <summary>
        /// Configures an <see cref="IConfigurationBuilder"/> to read Hazelcast configuration values from various sources.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="args">The command line args.</param>
        /// <param name="switchMappings">Command line switch mappings.</param>
        /// <param name="defaults">The defaults key-value configuration pairs.</param>
        /// <param name="keyValues">The optional key-value configuration pairs.</param>
        /// <param name="optionsFilePath">The optional path to the options file.</param>
        /// <param name="optionsFileName">The optional name of the options file.</param>
        /// <param name="environmentName">An optional environment name.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// <para>Adds support for hazelcast-specific sources. Does not add default support for other sources.</para>
        /// <para>If <paramref name="environmentName"/> is missing, the environment name is determined using
        /// the <c>DOTNET_ENVIRONMENT</c> and <c>ASPNETCORE_ENVIRONMENT</c> environment variables. If not
        /// specified, the environment name is <c>Production</c>.</para>
        /// </remarks>
        public static IConfigurationBuilder AddHazelcast(this IConfigurationBuilder configurationBuilder,
            string[] args,
            IDictionary<string, string> switchMappings = null,
            IEnumerable<KeyValuePair<string, string>> defaults = null,
            IEnumerable<KeyValuePair<string, string>> keyValues = null,
            string optionsFilePath = null,
            string optionsFileName = null,
            string environmentName = null)
        {
            if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));

            var sources = configurationBuilder.Sources;

            // this is always going to be the first one
            if (defaults != null)
                sources.Insert(0, new HazelcastMemoryConfigurationSource { InitialData = defaults }); // handles both standard and hazelcast syntax

            if (string.IsNullOrWhiteSpace(optionsFileName))
                optionsFileName = "hazelcast.json";

            // always process the hazelcast configuration file
            var i = sources.LastIndexOf(source => source is FileConfigurationSource);
            var (fullpath, extension) = GetHazelcastFilePath(optionsFilePath, optionsFileName);
            var fileSource1 = new JsonConfigurationSource { Optional = false, ReloadOnChange = false, Path = fullpath + extension};
            var fileSource2 = new JsonConfigurationSource { Optional = false, ReloadOnChange = false, Path = fullpath + '.' + DetermineEnvironment(environmentName) + extension };

            fileSource1.ResolveFileProvider(); // It has to be called here since we insert the file source directly to sources                                             
            fileSource2.ResolveFileProvider(); // which by passes the invocation of ResolveFileProvider.

            if (i != -1)
            {
                sources.Insert(i + 1, fileSource2);
                sources.Insert(i + 1, fileSource1);
            }
            else
            {
                sources.Add(fileSource1);
                sources.Add(fileSource2);
            }

            // process hazelcast-style environment variables if normal environment variables are processed
            i = sources.IndexOf(source => source is EnvironmentVariablesConfigurationSource);
            if (i != -1) sources.Insert(i + 1, new HazelcastEnvironmentVariablesConfigurationSource());

            // process hazelcast-style command line arguments if normal command line arguments are processed
            i = sources.LastIndexOf(source => source is CommandLineConfigurationSource);
            if (i != -1) sources.Insert(i + 1, new HazelcastCommandLineConfigurationSource { Args = args, SwitchMappings = switchMappings });

            // this is always going to be the last one
            if (keyValues != null)
                sources.Add(new HazelcastMemoryConfigurationSource { InitialData = keyValues }); // handles both standard and hazelcast syntax

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
