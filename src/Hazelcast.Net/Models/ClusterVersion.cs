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
namespace Hazelcast.Models
{
    /// <summary> Represents the version of the cluster. </summary> 
    public sealed class ClusterVersion
    {
        public const byte Unknown = 0;
        
        /// <summary> Initializes a new instance of the <see cref="ClusterVersion"/> class. </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public ClusterVersion(byte major, byte minor)
        {
            Major = major;
            Minor = minor;
        }

        /// <summary> Gets the major version number. </summary>
        public byte Major { get; }

        /// <summary> Gets the minor version number. </summary>
        public byte Minor { get; }

        /// <summary> Returns a string representation of the cluster version. </summary>
        public override string ToString() => $"{Major}.{Minor}";
        
        /// <summary>
        /// Either cluster version is know or not.
        /// </summary>
        public bool IsUnknown => Major == Unknown && Minor == Unknown;

        /// <summary> Parses a string representation of the cluster version. </summary>
        /// <param name="value">The string representation of the cluster version.</param>
        /// <returns>The parsed cluster version.</returns>
        public static ClusterVersion Parse(string value)
        {
            var parts = value.Split('.');
            if (parts.Length > 2)
                throw new FormatException("Invalid cluster version format.");

            if (!byte.TryParse(parts[0], out var major))
                throw new FormatException("Invalid cluster version format.");

            if (!byte.TryParse(parts[1], out var minor))
                throw new FormatException("Invalid cluster version format.");

            return new ClusterVersion(major, minor);
        }
/// <inheritdoc/>


        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            
            if (obj is ClusterVersion other)
            {
                return Major == other.Major && Minor == other.Minor;
            }

            return false;
        }
        /// <inheritdoc/>
        
        public override int GetHashCode()
        {
            return 31* Major + Minor;
        }
        
        

    }
}
