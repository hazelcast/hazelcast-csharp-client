// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
        /// <summary>
        /// Default group password.
        /// </summary>
        public const string DefaultGroupPassword = "dev-pass";

        /// <summary>
        /// Default group name.
        /// </summary>
        public const string DefaultGroupName = "dev";

        private string _name = DefaultGroupName;

        private string _password = DefaultGroupPassword;

        /// <summary>Creates a <see cref="GroupConfig"/> with default group-name and group-password.</summary>
        /// <remarks>Creates a <see cref="GroupConfig"/> with default group-name and group-password.</remarks>
        public GroupConfig()
        {
        }

        /// <summary>Creates a <see cref="GroupConfig"/> with the given group-name and default group-password</summary>
        /// <param name="name">the name of the group</param>
        /// <exception cref="System.ArgumentException">if name is null.</exception>
        public GroupConfig(string name)
        {
            SetName(name);
        }

        /// <summary>Creates a <see cref="GroupConfig"/> with the given group-name and group-password</summary>
        /// <param name="name">the name of the group</param>
        /// <param name="password">the password of the group</param>
        /// <exception cref="System.ArgumentException">if name or password is null.</exception>
        public GroupConfig(string name, string password)
        {
            SetName(name);
            SetPassword(password);
        }

        /// <inheritdoc />
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
            return (_name == null ? other._name == null : _name.Equals(other._name)) &&
                   (_password == null ? other._password == null : _password.Equals(other._password));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0) + 31*(_password != null ? _password.GetHashCode() : 0);
        }

        /// <summary>Gets the name of the group.</summary>
        /// <remarks>Gets the name of the group.</remarks>
        /// <returns>the name</returns>
        public string GetName()
        {
            return _name;
        }

        /// <summary>Gets the password to connec to to the group.</summary>
        /// <remarks>Gets the password to connec to to the group.</remarks>
        /// <returns>the password</returns>
        public string GetPassword()
        {
            return _password;
        }

        /// <summary>Sets the group name.</summary>
        /// <remarks>Sets the group name.</remarks>
        /// <param name="name">the name to set</param>
        /// <returns>the updated <see cref="GroupConfig"/>.</returns>
        /// <exception cref="System.ArgumentException">if name is null.</exception>
        public GroupConfig SetName(string name)
        {
            _name = ValidationUtil.IsNotNull(name, "group name");
            return this;
        }

        /// <summary>Sets the password.</summary>
        /// <remarks>Sets the password.</remarks>
        /// <param name="password">the password to set</param>
        /// <returns>the updated <see cref="GroupConfig"/>.</returns>
        /// <exception cref="System.ArgumentException">if password is null.</exception>
        public GroupConfig SetPassword(string password)
        {
            _password = ValidationUtil.IsNotNull(password, "group password");
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("GroupConfig [name=").Append(_name).Append(", password=");
            var len = _password.Length;
            for (var i = 0; i < len; i++)
            {
                builder.Append('*');
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
}