using System;
using NuGet.Versioning;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Provides constants for managing the server version during tests.
    /// </summary>
    public static class ServerVersion
    {
        // ReSharper disable once InconsistentNaming
        private const string DefaultVersionString = "4.0";

        /// <summary>
        /// Gets the default server version.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly NuGetVersion DefaultVersion = NuGetVersion.Parse(DefaultVersionString);

        /// <summary>
        /// Gets the name of the environment variable that can be used to override the default version.
        /// </summary>
        public const string EnvironmentVariableName = "HAZELCAST_SERVER_VERSION";

        /// <summary>
        /// Gets the server version indicated by the environment variable, or the default server version.
        /// </summary>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion()
        {
            var env = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            return NuGetVersion.TryParse(env, out var serverVersion)
                ? serverVersion
                : DefaultVersion;
        }
    }
}