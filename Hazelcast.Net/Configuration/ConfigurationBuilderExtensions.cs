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
        public static IConfigurationBuilder AddHazelcastFile(this IConfigurationBuilder configurationBuilder, string filePath, string fileName, string environmentName)
        {
            var fullpath = string.IsNullOrWhiteSpace(filePath)
                ? fileName
                : Path.Combine(filePath, fileName);

            var extension = Path.GetExtension(fullpath);
            fullpath = Path.GetFileNameWithoutExtension(fullpath);

            configurationBuilder
                .AddJsonFile(fullpath + extension, true)
                .AddJsonFile(fullpath + '.' + DetermineEnvironment(environmentName) + extension, true);

            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration values from environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddHazelcastEnvironmentVariables(this IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Add(new HazelcastEnvironmentVariablesConfigurationSource());
            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Hazelcast configuration values from the command line.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddHazelcastCommandLine(this IConfigurationBuilder configurationBuilder, string[] args)
        {
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
        public static IConfigurationBuilder AddHazelcast(this IConfigurationBuilder configurationBuilder, string[] args, IEnumerable<KeyValuePair<string, string>> keyValues = null, string optionsFilePath = null, string optionsFileName = null, string environmentName = null)
        {
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
            if (!string.IsNullOrWhiteSpace(aspnetcore))
                return aspnetcore;

            return "Production";
        }
    }
}
