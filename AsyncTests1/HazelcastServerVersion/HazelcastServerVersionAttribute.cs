using System;
using System.Reflection;
using NuGet.Versioning;

namespace AsyncTests1.HazelcastServerVersion
{
    /// <summary>
    /// Marks an assembly to force the Hazelcast server version.
    /// </summary>
    /// <remarks>
    /// <para>By default, the Hazelcast server version is indicated by the HAZELCAST_SERVER_VERSION
    /// environment variable. Use this attribute to force the version.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class HazelcastServerVersionAttribute : Attribute
    {
        private static NuGetVersion _staticServerVersion;
        private readonly NuGetVersion _serverVersion;

        /// <summary>
        /// Marks the assembly to force the Hazelcast server version.
        /// </summary>
        /// <param name="version">The version.</param>
        public HazelcastServerVersionAttribute(string version)
        {
            if (!NuGetVersion.TryParse(version, out _serverVersion))
                throw new ArgumentException("Invalid version.", nameof(version));
        }

        /// <summary>
        /// Gets the Hazelcast server version.
        /// </summary>
        public static NuGetVersion ServerVersion
        {
            get
            {
                if (_staticServerVersion != null)
                    return _staticServerVersion;

                var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<HazelcastServerVersionAttribute>();
                if (attribute != null)
                    return _staticServerVersion = attribute._serverVersion;

                var env = Environment.GetEnvironmentVariable("HAZELCAST_SERVER_VERSION");
                if (NuGetVersion.TryParse(env, out _staticServerVersion))
                    return _staticServerVersion;

                throw new InvalidOperationException("Could not determine server version.");
            }
        }
    }
}