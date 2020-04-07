using System;
using System.Reflection;
using NuGet.Versioning;

namespace Hazelcast.Testing.Conditions
{
    public sealed class ServerVersion
    {
        public const string EnvironmentVariableName = "HAZELCAST_SERVER_VERSION";

        static ServerVersion()
        {
            Reset();
        }

        public static NuGetVersion Version { get; set; }
        
        public static void Reset()
        {
            // do we really want this? executing vs?
            var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<ServerVersionAttribute>();
            if (attribute != null)
            {
                Version = attribute.Version;
                return;
            }

            var env = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            if (NuGetVersion.TryParse(env, out var version))
            {
                Version = version;
                return;
            }

            Version = NuGetVersion.Parse("0.0");
        }
    }

    /// <summary>
    /// Marks an assembly to force the Hazelcast server version.
    /// </summary>
    /// <remarks>
    /// <para>By default, the Hazelcast server version is indicated by the HAZELCAST_SERVER_VERSION
    /// environment variable. Use this attribute to force the version.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ServerVersionAttribute : Attribute
    {
        /// <summary>
        /// Marks the assembly to force the Hazelcast server version.
        /// </summary>
        /// <param name="version">The version.</param>
        public ServerVersionAttribute(string version)
        {
            if (!NuGetVersion.TryParse(version, out var v))
                throw new ArgumentException("Invalid version.", nameof(version));

            Version = v;
        }

        /// <summary>
        /// Gets the Hazelcast server version.
        /// </summary>
        public NuGetVersion Version { get; }
    }
}