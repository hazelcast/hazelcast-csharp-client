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

namespace Hazelcast.Data
{
    /// <summary>
    /// Represents the version of a cluster member.
    /// </summary>
    public class MemberVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public MemberVersion(byte major, byte minor, byte patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        public byte Major { get; }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        public byte Minor { get; }

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        public byte Patch { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}
