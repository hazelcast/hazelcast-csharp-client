using System.Text;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    /// <summary>Contains the configuration for Hazelcast groups.</summary>
    /// <remarks>
    ///     Contains the configuration for Hazelcast groups.
    ///     With groups it is possible to create multiple clusters where each cluster has its own group and doesn't
    ///     interfere with other clusters.
    /// </remarks>
    public sealed class GroupConfig
    {
        public const string DefaultGroupPassword = "dev-pass";

        public const string DefaultGroupName = "dev";

        private string name = DefaultGroupName;

        private string password = DefaultGroupPassword;

        /// <summary>Creates a GroupConfig with default group-name and group-password.</summary>
        /// <remarks>Creates a GroupConfig with default group-name and group-password.</remarks>
        public GroupConfig()
        {
        }

        /// <summary>Creates a GroupConfig with the given group-name and default group-password</summary>
        /// <param name="name">the name of the group</param>
        /// <exception cref="System.ArgumentException">if name is null.</exception>
        public GroupConfig(string name)
        {
            SetName(name);
        }

        /// <summary>Creates a GroupConfig with the given group-name and group-password</summary>
        /// <param name="name">the name of the group</param>
        /// <param name="password">the password of the group</param>
        /// <exception cref="System.ArgumentException">if name or password is null.</exception>
        public GroupConfig(string name, string password)
        {
            SetName(name);
            SetPassword(password);
        }

        /// <summary>Gets the name of the group.</summary>
        /// <remarks>Gets the name of the group.</remarks>
        /// <returns>the name</returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>Sets the group name.</summary>
        /// <remarks>Sets the group name.</remarks>
        /// <param name="name">the name to set</param>
        /// <returns>the updated GroupConfig.</returns>
        /// <exception cref="System.ArgumentException">if name is null.</exception>
        public GroupConfig SetName(string name)
        {
            this.name = ValidationUtil.IsNotNull(name, "group name");
            return this;
        }

        /// <summary>Gets the password to connec to to the group.</summary>
        /// <remarks>Gets the password to connec to to the group.</remarks>
        /// <returns>the password</returns>
        public string GetPassword()
        {
            return password;
        }

        /// <summary>Sets the password.</summary>
        /// <remarks>Sets the password.</remarks>
        /// <param name="password">the password to set</param>
        /// <returns>the updated GroupConfig.</returns>
        /// <exception cref="System.ArgumentException">if password is null.</exception>
        public GroupConfig SetPassword(string password)
        {
            this.password = ValidationUtil.IsNotNull(password, "group password");
            return this;
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0) + 31*(password != null ? password.GetHashCode() : 0);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!(obj is GroupConfig))
            {
                return false;
            }
            var other = (GroupConfig) obj;
            return (name == null ? other.name == null : name.Equals(other.name)) &&
                   (password == null ? other.password == null : password.Equals(other.password));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("GroupConfig [name=").Append(name).Append(", password=");
            int len = password.Length;
            for (int i = 0; i < len; i++)
            {
                builder.Append('*');
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
}